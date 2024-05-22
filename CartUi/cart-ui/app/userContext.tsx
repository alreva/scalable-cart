'use client'
import React, { ReactNode, createContext, useContext, useEffect, useState } from 'react';

export interface User {
  name: string;
  id: number;
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

export const UserProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | undefined>();

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      setUser(JSON.parse(userString));
    }
  }, []);

  useEffect(() => saveUser(user), [user]);

  return (
    <UserContext.Provider value={{ user, setUser: u => { saveUser(u); setUser(u); } }}>
      {children}
    </UserContext.Provider>
  );
}
