import React, { useEffect } from 'react';
import LoginForm from '../components/LoginForm';
import { Link, useNavigate } from 'react-router-dom';



const LandingPage = () => {
    const navigate = useNavigate();

    useEffect(() => {
        const user = JSON.parse(localStorage.getItem('user'));
        if (user) {
            navigate("/dashboard");
        }
    }, [navigate]);


    const handleLogin = async (token) => {
        const response = await fetch('/api/account/getProfilePageData', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            credentials: 'include',
        });

        if (response.ok) {
            const userData = await response.json();

            const loggedInUser = {
                token,
                firstName: userData.user.firstName,
                username: userData.user.userName,
                profilePicture: userData.user.profilePicture
            };
            localStorage.setItem('user', JSON.stringify(loggedInUser));
            console.log(loggedInUser);
        }
        localStorage.setItem('token', token);
        navigate("/dashboard");
    };

    return (

        <div className="container mt-4">
            <LoginForm onLogin={handleLogin} />
            <div className="d-flex flex-column align-items-center justify-content-center">
                <strong>
                    <p className="my-text"> If you don't have an account, you can register here:</p>
                </strong>
                <Link to="/register" className="btn btn-primary">Register</Link>
            </div>
            <div className="mt-5">
                <div className="my-card h-100 w-100 mx-auto">
                    <img className="my-icons" src='https://cdn-icons-png.flaticon.com/128/4256/4256662.png' alt="Icon" />
                    <p className="my-card-text">
                        The Expense Tracker Application is designed to help you effectively manage your finances with ease. Whether you're tracking daily expenses,
                        managing multiple wallets, or organizing your financial transactions, this application provides a comprehensive solution for your financial management needs.</p>
                </div>
            </div>
        </div>
    );
}

export default LandingPage;
