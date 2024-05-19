'use client'
import React from "react";
import { Nav, Navbar, NavDropdown } from "react-bootstrap";
import {usePathname} from "next/navigation";

export const Menu: React.FC = () => {
  
  var pathName = usePathname();
    
  return (
      <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
              <Nav.Link href="/">Home</Nav.Link>
              <Nav.Link href="/catalog">Catalog</Nav.Link>
              <Nav.Link href="/cart">Cart</Nav.Link>
              <Nav.Link href="/about">About</Nav.Link>
          </Nav>
      </Navbar.Collapse>
  );
};