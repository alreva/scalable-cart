"use client";
import React, {
  ReactNode,
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { CartDetailsDto, CartResponseDto } from "./cartDto";
import { usePathname } from "next/navigation";
import { API_URL } from "../env";

export const UserProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  console.log("UserProvider");
  const [user, setUser] = useState<User | undefined>();
  const [isCartRelevant, setCartIsRelevant] = useState<boolean>(false);
  const pathName = usePathname();

  useEffect(() => {
    if (isCartRelevant) {
      console.log("Cart is already relevant.");
      return;
    }

    const savedUser = loadUser();
    console.log("user ID: ", savedUser?.id);
    if (savedUser && savedUser.id > 0) {
      fetch(`/api/cart/${savedUser.id}`)
        .then((res) => res.json() as Promise<CartResponseDto>)
        .then((data) => {
          subscribeToNotifications(savedUser.id, (details) => {
            console.log("Received updated cart details: ", details);
            const updatedUser = { ...savedUser, cartDetails: details };
            saveUser(updatedUser);
            setUser(updatedUser);
          });
          const userAndCart = { ...savedUser, cartDetails: data.details };
          console.log("User and cart details loaded: ", userAndCart);
          saveUser(userAndCart);
          setCartIsRelevant(true);
          setUser(userAndCart);
        });
    }
  }, [isCartRelevant, pathName]);

  return (
    <UserContext.Provider
      value={{
        user,
        setUser: (u) => {
          saveUser(u);
          setUser(u);
        },
      }}
    >
      {children}
    </UserContext.Provider>
  );
};

export interface User {
  name: string;
  id: number;
  cartDetails: CartDetailsDto | undefined;
}

export interface UserContextProps {
  user: User | undefined;
  setUser: (user: User | undefined) => void;
}

export interface UserContextProps {
  user: User | undefined;
  setUser: (user: User | undefined) => void;
}

const UserContext = createContext<UserContextProps | undefined>(undefined);

export const useUser = (): UserContextProps => {
  const context = useContext(UserContext);
  if (!context) {
    throw new Error("useUser must be used within a UserProvider");
  }
  return context;
};

const loadUser = (): User | undefined => {
  if (localStorage === undefined) {
    return undefined;
  }
  const userString = localStorage.getItem("user");
  if (userString) {
    return JSON.parse(userString);
  }
  return undefined;
};

const saveUser = (user: User | undefined) => {
  if (user) {
    localStorage.setItem("user", JSON.stringify(user));
  } else {
    localStorage.removeItem("user");
  }
};

function isCartDetails(arg: any): arg is CartDetailsDto {
  return arg && arg.cartId && arg.lineItems && arg.totalPrice;
}

const subscribeToNotifications = (
  cartId: number,
  detailsHandler: (details: CartDetailsDto) => void
) => {
  const connection = new HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/cart?cartId=${cartId}`, {
      withCredentials: false,
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(LogLevel.Information)
    .build();

  connection
    .start()
    .then(() => {
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
    })
    .catch((error) => {
      console.error("Error establishing SignalR connection:", error);
    });
};
