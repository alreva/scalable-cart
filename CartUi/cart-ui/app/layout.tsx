"use client";
import "bootstrap/dist/css/bootstrap.min.css";
import React from "react";
import Head from "next/head";
import { Container, Row, Col, Nav, Navbar, NavDropdown } from "react-bootstrap";
import { UserProvider, useUser } from "./components/userContext";
import MiniCart from "./components/minicart";

interface LayoutProps {
  children: React.ReactNode;
}

const App: React.FC<LayoutProps> = ({ children }) => {
  return (
    <html data-bs-theme="dark">
      <head>
        <Head>
          <title>Cart UI</title>
          <meta name="description" content="A Bootstrap layout with Next.js" />
        </Head>
      </head>
      <body>
        <UserProvider>
          <header className="navbar navbar-expand-lg bd-navbar sticky-top">
            <TopNavigation />
          </header>
          <Container
            fluid
            className="container-xxl bd-gutter mt-3 my-md-4 bd-layout"
          >
            <Row>
              <Col md={3}>
                <Nav className="flex-column">
                  <Nav.Link href="/">Dashboard</Nav.Link>
                  <Nav.Link href="/profile">Profile</Nav.Link>
                  <Nav.Link href="/settings">Settings</Nav.Link>
                </Nav>
              </Col>
              <Col md={6}>
                <main>{children}</main>
              </Col>
            </Row>
          </Container>
        </UserProvider>
      </body>
    </html>
  );
};

export function TopNavigation() {
  const { user } = useUser();
  return (
    <Navbar
      className="container-xxl bd-gutter flex-wrap flex-lg-nowrap"
      expand="lg"
    >
      <Container fluid>
        <Navbar.Brand href="/">Scalable Cart</Navbar.Brand>
      </Container>
      <Navbar.Collapse id="basic-navbar-nav">
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Nav className="me-auto">
          <Nav.Link href="/">Home</Nav.Link>
          <Nav.Link href="/catalog">Catalog</Nav.Link>
          <Nav.Link href="/cart">Cart</Nav.Link>
          <Nav.Link href="/about">About</Nav.Link>
          {user && (
            <NavDropdown title={"User: " + user.name} id="basic-nav-dropdown">
              <NavDropdown.Item href="/logout">Logout</NavDropdown.Item>
            </NavDropdown>
          )}
          {user && <MiniCart cartDetails={user.cartDetails} />}
          {!user && <Nav.Link href="/login">Login</Nav.Link>}
        </Nav>
      </Navbar.Collapse>
    </Navbar>
  );
}

export default App;
