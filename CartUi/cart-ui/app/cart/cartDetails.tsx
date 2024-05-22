'use client'
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import React, { useEffect, useState } from "react";
import { CartDetails as CartDetailsModel, CartResponse, LineItem as LineItemModel } from './cartDto';
import { Table } from 'react-bootstrap';

export function isCartDetails(arg: any): arg is CartDetailsModel {
  return arg && arg.cartId && arg.lineItems && arg.totalPrice;
}

export const subscribeToNotifications = (
  cartId: number,
  setLiveDetails: React.Dispatch<React.SetStateAction<CartDetailsModel>>
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
        setLiveDetails(data);
      }

    });
  }).catch((error) => {
    console.error("Error establishing SignalR connection:", error);
  });
};

export const CartDetails: React.FC<CartResponse> = ({
  details,
}) => {

  const[liveDetails, setLiveDetails] = useState(details);

  console.log("CartDetails: ", liveDetails);

  useEffect(() => {
    if (liveDetails.cartId > 0) {
      subscribeToNotifications(liveDetails.cartId, setLiveDetails);
    }
  }, [liveDetails]);

  const { lineItems, totalPrice } = liveDetails;

  return (
    <>
      {lineItems.length > 0 && (
        <Table striped>
          <thead>
            <tr>
              <th>Product Name</th>
              <th className="text-end">Price</th>
              <th className="text-end">Quantity</th>
            </tr>
          </thead>
          <tbody>
            {lineItems.map((item) => (
              <LineItem key={item.productName} {...item} />
            ))}
          </tbody>
          <tfoot className="table-group-divider">
            <tr>
              <td colSpan={3} align="right">
                Total Price: {totalPrice}
              </td>
            </tr>
          </tfoot>
        </Table>
      )}
      {lineItems.length === 0 && <p>No items in the cart</p>}
    </>
  );
};

export const LineItem: React.FC<LineItemModel> = ({
  productName,
  price,
  quantity,
}) => {
  return (
    <tr>
      <td>{productName}</td>
      <td align="right">{price}</td>
      <td align="right">{quantity}</td>
    </tr>
  );
};
  