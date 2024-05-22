"use client";
import React, { useEffect, useState } from "react";
import { CartResponse } from "./cartDto";
import { CartDetails } from "./cartDetails";
import { useUser } from "../userContext";
import { Console } from "console";

const EmptyCart = () => (
  <>
    <h1>Cart Details</h1>
    <CartDetails
      {...{ path: "", details: { cartId: -1, lineItems: [], totalPrice: 0 } }}
    />
  </>
);

export default function Cart() {
  const { user } = useUser();
  const [data, setData] = useState<CartResponse | undefined>(undefined);

  console.log("User: ", user);

  useEffect(() => {
    if (!user) {
      return;
    }

    fetch("/cart/api/?id=" + user.id.toString())
      .then((res) => res.json() as Promise<CartResponse>)
      .then((data) => {
        console.log("XHR data: ", data);
        setData(data);
      });
  }, [user]);

  if (!data) {
    return <EmptyCart />;
  }

  return (
    <>
      <h1>Cart Details</h1>
      <CartDetails {...data} />
    </>
  );
}
