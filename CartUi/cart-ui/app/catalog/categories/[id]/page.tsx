"use client";
import { useEffect, useState } from "react";
import { Container, Row, Col, Card, Pagination, Button, Badge } from "react-bootstrap";
import { useSearchParams, useRouter } from "next/navigation";
import { useUser } from "@/app/components/userContext";
import { url } from "inspector";

export const pageSize = 8;
export const pagerSize = 10;

const CategoryPage = (
  { params }: { params: { id: string } },
) => {
  const [data, setData] = useState<{category: {name: string}; products: ProductDto[]; totalProducts: number }>({
    category: {name: ""},
    products: [],
    totalProducts: 0,
  });
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
  const pns = [];
  const isAuthenticated = !!user;
  const cartId = isAuthenticated ? user.id : null;

  const handlePageSelect = (pageNumber: number) => () => {
    console.log("pageNumber:", pageNumber);
    router.push(`/catalog/categories/${id}/?page=${pageNumber}`, {
      scroll: true,
    });
  };

  const pn = (i: number) => (
    <Pagination.Item key={i} active={i === page} onClick={handlePageSelect(i)}>
      {i}
    </Pagination.Item>
  );
  const el = (i: number) => (
    <Pagination.Ellipsis key={i} onClick={handlePageSelect(i)} />
  );
  const fst = () => (
    <Pagination.First key={1} active={1 === page} onClick={handlePageSelect(1)}>
      {1}
    </Pagination.First>
  );
  const lst = () => (
    <Pagination.Last
      key={totalPages}
      active={totalPages === page}
      onClick={handlePageSelect(totalPages)}
    >
      {totalPages}
    </Pagination.Last>
  );

  if (page <= pagerSize) {
    for (let i = 1; i <= pageSize; i++) {
      pns.push(pn(i));
    }
    pns.push(el(11));
    pns.push(lst());
  } else if (totalPages - page <= pagerSize) {
    pns.push(fst());
    pns.push(el(totalPages - 11));
    for (let i = totalPages - 10; i <= totalPages; i++) {
      pns.push(pn(i));
    }
  } else {
    pns.push(fst());
    pns.push(el(page - 4));
    for (let i = page - 3; i <= page + 3; i++) {
      pns.push(pn(i));
    }
    pns.push(el(page + 4));
    pns.push(lst());
  }

  const handleAddToCart = (product: ProductDto) => {
    const { id } = product;
    console.log("productId:", id);
    const postData = async () => {
      try {
        const response = await fetch(`/api/cart/${cartId}/add-product`, {
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
        {products?.length > 0 && <Pagination>{pns}</Pagination>}
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
