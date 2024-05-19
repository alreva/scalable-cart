// components/Layout.tsx
'use client'
import 'bootstrap/dist/css/bootstrap.min.css';
import React from 'react';
import Head from 'next/head';
import { Container, Row, Col, Nav, Navbar, NavDropdown } from 'react-bootstrap';
import { Menu } from './menu';

interface LayoutProps {
  children: React.ReactNode;
}

const App: React.FC<LayoutProps> = ({ children }) => {
    return (
        <html>
          <head>
            
          </head>
          <body>
            <Layout>
              {children}
            </Layout>
          </body>
        </html>
    );
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  return (
      <html>
        <head>
          <Head>
            <title>Cart UI</title>
            <meta name="description" content="A Bootstrap layout with Next.js" />
          </Head>
        </head>
        <body>
          <Navbar bg="dark" variant="dark" expand="lg">
            <Navbar.Brand href="/">Next.js Bootstrap</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Menu />
          </Navbar>
          <Container fluid>
            <Row>
              <Col md={2}>
                <Nav className="flex-column">
                  <Nav.Link href="/">Dashboard</Nav.Link>
                  <Nav.Link href="/profile">Profile</Nav.Link>
                  <Nav.Link href="/settings">Settings</Nav.Link>
                </Nav>
              </Col>
              <Col md={10}>
                <main>{children}</main>
              </Col>
            </Row>
          </Container>
        </body>
      </html>
  );
};

export default App;
