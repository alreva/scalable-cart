"use client";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import React, { useEffect, useState } from "react";
import {
  CartDetailsDto,
  LineItemDto,
} from "../components/cartDto";
import { Table } from "react-bootstrap";
import { formatPrice } from "../components/formatPrice";

export const CartDetails: React.FC<CartDetailsDto> = ({ lineItems, totalPrice }) => {
  return (
    <>
      {lineItems.length === 0 && <EmptyCart />}
      {lineItems.length > 0 && (
        <Table striped>
          <thead>
            <tr>
              <th>#</th>
              <th className="text-end">Price</th>
              <th className="text-end">Quantity</th>
            </tr>
          </thead>
          <tbody>
            {lineItems.map((item) => (
              <LineItem key={item.productId} {...item} />
            ))}
          </tbody>
          <tfoot className="table-group-divider">
            <tr>
              <td colSpan={3} align="right">
                Total Price: {formatPrice(totalPrice)}
              </td>
            </tr>
          </tfoot>
        </Table>
      )}
    </>
  );
};

export const LineItem: React.FC<LineItemDto> = ({
  productId,
  price,
  quantity,
}) => {
  return (
    <tr>
      <td>{productId}</td>
      <td align="right">{formatPrice(price)}</td>
      <td align="right">{quantity}</td>
    </tr>
  );
};

export const EmptyCart = () => <p>No items in the cart</p>;
