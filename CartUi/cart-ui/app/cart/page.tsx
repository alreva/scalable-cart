import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Table } from 'react-bootstrap';
import { CartDetails, CartResponse, LineItem } from './cartDto';

export default async function Cart() {

  const res = await fetch('http://localhost:5254/cart/1');
  const data = await res.json() as CartResponse;
  console.log(data);
  const cart = data.details;
  const { lineItems, totalPrice } = cart;

  return (
    <>
      <h1>Cart Details</h1>
      {lineItems.length > 0 && (
        <Table striped>
          <thead>
            <tr>
              <th>Product Name</th>
              <th className='text-end'>Price</th>
              <th className='text-end'>Quantity</th>
            </tr>
          </thead>
          <tbody>
            {lineItems.map((item) => (<LineItem key={item.productName} {...item} />))}
          </tbody>
          <tfoot className='table-group-divider'>
            <tr>
              <td colSpan={3} align='right'>Total Price: {totalPrice}</td>
            </tr>
          </tfoot>
        </Table>
      )}
      {lineItems.length === 0 && <p>No items in the cart</p>}
    </>
  );
}

const LineItem: React.FC<LineItem> = ({productName, price, quantity}) => {
  return (
    <tr>
      <td>{productName}</td>
      <td align='right'>{price}</td>
      <td align='right'>{quantity}</td>
    </tr>
  );
}

