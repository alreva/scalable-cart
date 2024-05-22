"use client";
import React, { useEffect, useState } from "react";
import { CartResponseDto } from "../components/cartDto";
import { CartDetails, EmptyCart } from "./cartDetails";
import { useUser } from "../components/userContext";

export default function Cart() {
  const { user } = useUser();
  const cartId = user?.id || 0;
  const cartDetails = user?.cartDetails;

  return (
    <>
      <h1>Cart Details</h1>
      { cartId > 0 && cartDetails && cartDetails.lineItems.length > 0 &&  <CartDetails {...cartDetails} /> }
      { (cartId === 0 || !cartDetails || cartDetails.lineItems.length === 0) && <EmptyCart /> }
    </>
  );
}
