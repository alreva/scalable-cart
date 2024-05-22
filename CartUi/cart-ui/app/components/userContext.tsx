'use client'
import React, { ReactNode, createContext, useContext, useEffect, useState } from 'react';
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { CartDetailsDto, CartResponseDto } from './cartDto';

export const UserProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | undefined>();

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user = JSON.parse(userString);

      fetch("/cart/api/?id=" + user.id.toString())
      .then((res) => res.json() as Promise<CartResponseDto>)
      .then((data) => {
        console.log("XHR data: ", data);
        const userAndCart = { ...user, cartDetails: data.details };
        console.log("User and cart: ", userAndCart);
        setUser(userAndCart);
        subscribeToNotifications(user.id, (details) => {
          console.log("Received updated cart details: ", details);
          setUser({ ...userAndCart, cartDetails: details });
        });
      });
    }
  }, []);

  return (
    <UserContext.Provider value={{ user, setUser: saveUser }}>
      {children}
    </UserContext.Provider>
  );
}

export interface User {
  name: string;
  id: number;
  cartDetails: CartDetailsDto | undefined;
}

export interface UserContextProps {
  user: User | undefined;
  setUser: (user: User | undefined) => void;
}

export const UserContext = createContext<UserContextProps | undefined>(undefined);

export const useUser = (): UserContextProps => {
  const context = useContext(UserContext);
  if (!context) {
    throw new Error('useUser must be used within a UserProvider');
  }
  return context;
};

export const saveUser = (user: User | undefined) => {
  if (user) {
    localStorage.setItem('user', JSON.stringify(user));
  } else {
    localStorage.removeItem('user');
  }
}

export function isCartDetails(arg: any): arg is CartDetailsDto {
  return arg && arg.cartId && arg.lineItems && arg.totalPrice;
}

export const subscribeToNotifications = (
  cartId: number,
  detailsHandler: (details: CartDetailsDto) => void
) => {
  const connection = new HubConnectionBuilder()
    .withUrl("http://localhost:5254/hubs/cart?cartId=" + cartId, {
        withCredentials: false,
    })
    .configureLogging(LogLevel.Information)
    .build();

  connection.start().then(() => {
    console.log("SignalR connection established.");

    // Subscribe to the desired hub method
    connection.on("ReceiveMessage", (data: any) => {
      // Handle the received notification data
      console.log("Received notification:", data);
      // TODO: Add your custom logic here

      if (isCartDetails(data)) {
        detailsHandler(data);
      }

    });
  }).catch((error) => {
    console.error("Error establishing SignalR connection:", error);
  });
};
