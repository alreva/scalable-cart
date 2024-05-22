'use client';
import React, { useEffect } from 'react';
import { useUser } from '../components/userContext';
import { useRouter } from 'next/navigation';

const LogoutPage: React.FC = () => {
    const router = useRouter();
    const { setUser } = useUser();

    useEffect(() => {
        setUser(undefined);
        router.push('/login');
    });

    return (
        <div>
            <h1>Logging out...</h1>
            {/* You can add a loading spinner or any other UI elements here */}
        </div>
    );
};

export default LogoutPage;