"use client";
import React from "react";
import { Navbar, Nav, Badge } from "react-bootstrap";
import { User, useUser } from "./userContext";
import { CartDetailsDto } from "./cartDto";
import { formatPrice } from "./formatPrice";

const MiniCart: React.FC<{ cartDetails: CartDetailsDto | undefined }> = ({
  cartDetails,
}) => {
  const totalItems = cartDetails?.lineItems.length || 0;
  const totalPrice = cartDetails?.totalPrice || 0;
  return (
    <Navbar.Collapse id="basic-navbar-nav">
      <Nav className="ml-auto">
        <Nav.Link href="/cart">
          <i className="fa fa-shopping-cart"></i>
          <Badge bg="danger">
            {totalItems} | {formatPrice(totalPrice)}
          </Badge>
        </Nav.Link>
      </Nav>
    </Navbar.Collapse>
  );
};

export default MiniCart;
