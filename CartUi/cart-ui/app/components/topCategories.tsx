'use client';
import { useEffect, useState } from "react";
import { Nav } from "react-bootstrap";


export default function TopCategories() {

    const [categories, setCategories] = useState<CategoryDto[]>([]);

    console.log('TopCategories', categories);

    useEffect(() => {
      const topCategories = 
        fetch("/api/catalog/top-categories")
        .then((response) => response.json() as Promise<TopCategoriesDto>)
        .then((data) => setCategories(data.categories || []));
    }, []);

    if (categories.length === 0) {
        return (<></>)
    };

    return (
        <Nav className="flex-column">
            {categories.length > 0 && categories.map((category) => (
                <Nav.Link key={category.name} href={`/catalog/categories/${category.name}`}>
                    {category.name}
                </Nav.Link>
            ))}
        </Nav>
    );
}

export interface TopCategoriesDto {
    categories: CategoryDto[];
}

export interface CategoryDto {
    name: string;
}