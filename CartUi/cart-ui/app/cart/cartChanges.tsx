'use client'
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import React, { useEffect } from "react";

export default function CartMessages() {
    useEffect(() => {
        subscribeToNotifications();
    }, []);

    return (
        <div>
            <h1>Cart Messages</h1>
        </div>
    );
}

export const subscribeToNotifications = () => {
  const connection = new HubConnectionBuilder()
    .withUrl("http://localhost:5254/hubs/cart?cartId=1", {
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
    });
  }).catch((error) => {
    console.error("Error establishing SignalR connection:", error);
  });
};