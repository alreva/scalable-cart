"use client";
import { useEffect, useState } from "react";
import { Container, Row, Col, Card, Pagination, Button, Badge } from "react-bootstrap";
import { useSearchParams, useRouter } from "next/navigation";
import { useUser } from "@/app/components/userContext";
import { Pager } from "@/app/components/pager";

const CategoryPage = (
  { params }: { params: { id: string } },
) => {
  const [data, setData] = useState<{category: {name: string}; products: ProductDto[]; totalProducts: number }>({
    category: {name: ""},
    products: [],
    totalProducts: 0,
  });

  const pageSize = 8;
  const searchParams = useSearchParams();
  const router = useRouter();
  const { user } = useUser();

  const { id } = params;
  const page = parseInt(searchParams.get("page") || "1");
  const skip = (page - 1) * pageSize;

  const { category, products, totalProducts } = data;
  const { name: categoryName } = category;

  useEffect(() => {
    console.log("id:", id);
    const fetchData = async () => {
      try {
        const response = await fetch(
          `/api/catalog/category/${id}/products?skip=${skip}&take=${pageSize}`
        );
        const data = (await response.json()) as {
          category: { name: string };
          products: ProductDto[];
          totalProducts: number;
        };
        setData(data);
      } catch (error) {
        console.error("Error fetching products:", error);
      }
    };
    fetchData();
  }, [skip]);

  const totalPages = Math.ceil(totalProducts / pageSize);
  const isAuthenticated = !!user;
  const cartId = isAuthenticated ? user.id : null;

  const handlePageSelect = (pageNumber: number) => () => {
    console.log("pageNumber:", pageNumber);
    router.push(`/catalog/categories/${id}/?page=${pageNumber}`, {
      scroll: true,
    });
  };

  const handleAddToCart = (product: ProductDto) => {
    const { id } = product;
    console.log("productId:", id);
    const postData = async () => {
      try {
        const response = await fetch(`/api/cart/${cartId}/products`, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            productId: id,
            price: product.price,
          }),
        });
        const data = await response.json();
        console.log("data:", data);
      } catch (error) {
        console.error("Error adding to cart:", error);
      }
    };

    postData();
  };

  return (
    <div>
      <h1>{categoryName}</h1>
      <Container>
        <Row>
          {products.map((product) => (
            <Col key={product.id} className="mb-4" md={6}>
              <Card>
                <Card.Body>
                  <Card.Title><Badge bg="info">{product.id}</Badge> {product.name}</Card.Title>
                  <Card.Text>{product.description}</Card.Text>
                  <Card.Text>Brand: {product.brand}</Card.Text>
                  <Card.Text>Price: ${product.price}</Card.Text>
                  { isAuthenticated && <div
                    style={{
                      position: "absolute",
                      bottom: "10px",
                      right: "10px",
                    }}
                  >
                    <Button variant="primary" onClick={() => handleAddToCart(product)}>+</Button>
                  </div>}
                </Card.Body>
              </Card>
            </Col>
          ))}
        </Row>
        {products?.length > 0 && <Pager {...{ pageSize, totalPages, page, handlePageSelect }}  />}
      </Container>
    </div>
  );
};

export default CategoryPage;

export interface ProductDto {
  id: number;
  name: string;
  price: number;
  description: string;
  brand: string;
  category: string;
}
