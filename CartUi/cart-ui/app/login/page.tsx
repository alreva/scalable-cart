'use client';
import React, { useState } from 'react';
import { useUser } from '../components/userContext';
import { Form, Button } from 'react-bootstrap';
import { useRouter } from 'next/navigation';

const LoginPage: React.FC = () => {
    const { user, setUser } = useUser();
    const [userName, setUserName] = useState(user?.name || '');
    const [userId, setUserId] = useState(user && user.id.toString() || '');
    const router = useRouter();

    const handleUserNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setUserName(event.target.value);
    };

    const handleUserIdChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        setUserId(event.target.value);
    };

    const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        // Perform login logic here
        console.log('User Name:', userName);
        console.log('User ID:', userId);

        setUser({ name: userName, id: parseInt(userId), cartDetails: undefined});
        router.push('/');
    };

    return (
        <div>
            <h1>Login Page</h1>
            <Form onSubmit={handleSubmit}>
                <Form.Group controlId="userName">
                    <Form.Label>User Name:</Form.Label>
                    <Form.Control
                        type="text"
                        value={userName}
                        onChange={handleUserNameChange}
                    />
                </Form.Group>
                <Form.Group controlId="userId">
                    <Form.Label>User ID:</Form.Label>
                    <Form.Control
                        type="text"
                        value={userId}
                        onChange={handleUserIdChange}
                    />
                </Form.Group>
                <Button type="submit">Login</Button>
            </Form>
        </div>
    );
};

export default LoginPage;