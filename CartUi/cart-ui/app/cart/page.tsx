'use client';
import React, { useEffect, useState } from 'react';
import { Container, Row, Col, Table } from 'react-bootstrap';
import { CartDetails, LineItem } from './cartDto';

export default function Cart() {

  const [cart, setCart] = useState<CartDetails>();  

  useEffect(() => {
    const callApi = async () => {
      const response = await fetch('/cart/api');
      const data = await response.json() as CartDetails;
      setCart(data);
    };

    callApi();
  }, [cart?.cartId]);

  if (!cart) { return (<></>) }

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

