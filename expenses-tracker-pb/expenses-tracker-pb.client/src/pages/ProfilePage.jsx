import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const ProfilePage = () => {
    const [userData, setUserData] = useState({
        user: {
            firstName: '',
            lastName: '',
            userName: '',
            email: '',
            password: '******',
            twoFactorEnabled: false,
        },
        logins: [],
    });
    const [isEditing, setIsEditing] = useState(false);
    const [validationErrors, setValidationErrors] = useState({});
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [alertMessage, setAlertMessage] = useState('');
    const navigate = useNavigate();
    const [confirmation, setConfirmation] = useState("");

    const handleConfitmationChange = (e) => {
        setConfirmation(e.target.value);
    };

    useEffect(() => {
        fetchUserData();
        checkUserLogin();
    }, []);

    useEffect(() => {
        if (!isLoggedIn) {
            setAlertMessage("You are not logged in. Redirecting to login page...");
            const redirectTimer = setTimeout(() => {
                navigate("/");
            }, 3000);

            return () => clearTimeout(redirectTimer);
        }
    }, [isLoggedIn, navigate]);

    const fetchUserData = async () => {
        try {
            const response = await fetch('/api/account/GetProfilePageData', {
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Failed to fetch user data');
            }

            const data = await response.json();
            data.user.password = '******';
            setUserData(data);
        } catch (error) {
            console.error('Error during fetching user data:', error);
        }
    };

    const checkUserLogin = async () => {
        try {
            const response = await fetch("/api/account/user", {
                method: "POST",
                credentials: "include",
            });

            if (response.ok) {
                setIsLoggedIn(true);
            } else {
                setIsLoggedIn(false);
            }
        } catch (error) {
            console.error("Error checking user login:", error);
        }
    };

    const handleEditClick = () => {
        setIsEditing(true);
        fetchUserData();
    };

    const handleSaveClick = async () => {
        setValidationErrors({});
        try {
            const validationErrors = validateUserData();
            if (Object.keys(validationErrors).length > 0) {
                setValidationErrors(validationErrors);
                return;
            }

            setIsEditing(false);
            setValidationErrors({});

            const response = await fetch('/api/account/UpdateProfilePageData', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    userId: userData.user.id,
                    firstName: userData.user.firstName,
                    lastName: userData.user.lastName,
                    userName: userData.user.userName,
                    email: userData.user.email,
                    password: userData.user.password
                }),
            });

            if (!response.ok) {
                throw new Error('Failed to save data');
            }

            fetchUserData();
        } catch (error) {
            console.error('Error during saving user data:', error);
        }
    };

    const handleCancelClick = async () => {
        setIsEditing(false);
        fetchUserData();
    };

    const handleInputChange = (e) => {
        const { name, value, type, checked } = e.target;
        setUserData((prevUserData) => ({
            ...prevUserData,
            user: {
                ...prevUserData.user,
                [name]: type === 'checkbox' ? checked : value,
            },
        }));
    };

    const validateUserData = () => {
        const errors = {};

        if (!userData.user.firstName.trim()) {
            errors.firstName = 'First name is required';
        }

        if (!userData.user.lastName.trim()) {
            errors.lastName = 'Last name is required';
        }

        if (!userData.user.userName.trim()) {
            errors.userName = 'Username is required';
        }

        if (!userData.user.email.trim()) {
            errors.email = 'Email is required';
        } else if (!isValidEmail(userData.user.email)) {
            errors.email = 'Invalid email format';
        }

        if (!userData.user.password.trim()) {
            errors.password = 'Password is required';
        } else if (!isValidPassword(userData.user.password)) {
            errors.password = 'Invalid password format';
        }

        if (userData.user.password != confirmation) {
            errors.confirmation = 'Both passwords have to be the same';
        }

        return errors;
    };

    const isValidEmail = (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    };

    const isValidPassword = (password) => {
        if (password.length < 8) {
            return false;
        } else if (!/[A-Z]/.test(password)) {
            return false;
        } else if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
            return false;
        }
        else if (!/[0-9]/.test(password)) {
            return false;
        }
        
        return true;
    };

    const checkPasswordComplexity = () => {
        const complexityRules = [
            { condition: userData.user.password.length >= 8, message: "at least 8 characters long" },
            { condition: /[A-Z]/.test(userData.user.password), message: "contains a big letter" },
            { condition: /[0-9]/.test(userData.user.password), message: "contains a number" },
            { condition: /[!@#$%^&*(),.?":{}|<>]/.test(userData.user.password), message: "contains a special sign" },
        ];

        return complexityRules.map((rule, index) => (
            <li key={index} style={{ color: rule.condition ? '#26bf1b' : '#f30000' }}>
                {rule.message}{index !== complexityRules.length - 1 && ", "}
            </li>
        ));
    };

    return (
        <div className="container">
            {isLoggedIn ? (
                <>
                    <h2 className="mt-4">Account details:</h2>
                    <div className="profile__component">
                        <table>
                            <tbody>
                                <tr>
                                    <td>
                                        <label>First Name:</label>
                                    </td>
                                    <td>
                                        <label>Last Name:</label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                <input
                                                    type="text"
                                                    name="firstName"
                                                    className="form-control"
                                                    value={userData.user.firstName}
                                                    onChange={handleInputChange}
                                                />
                                                <div className="error">{validationErrors.firstName}</div>
                                            </>
                                        ) : (
                                            userData.user.firstName
                                        )}
                                    </td>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                <input
                                                    type="text"
                                                    name="lastName"
                                                    className="form-control"
                                                    value={userData.user.lastName}
                                                    onChange={handleInputChange}
                                                />
                                                <div className="error">{validationErrors.lastName}</div>
                                            </>
                                        ) : (
                                            userData.user.lastName
                                        )}
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <label>Username:</label>
                                    </td>
                                    <td>
                                        <label>Email:</label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                {userData.user.userName}
                                            </>
                                        ) : (
                                            userData.user.userName
                                        )}
                                    </td>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                {userData.user.email}
                                            </>
                                        ) : (
                                            userData.user.email
                                        )}
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <label>Password:</label>
                                    </td>
                                    <td>
                                        <label>TwoFactorEnabled:</label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                <input
                                                    type="password"
                                                    name="password"
                                                    className="form-control"
                                                    value={userData.user.password}
                                                    onChange={handleInputChange}
                                                />
                                                <div className="error">{validationErrors.password}</div>
                                                <ul>{checkPasswordComplexity()}</ul>
                                            </>
                                        ) : (
                                            userData.user.password
                                        )}
                                    </td>
                                    <td>
                                        {userData.user.twoFactorEnabled ? "Yes" : "No"}
                                     </td>
                                        
                                </tr>
                                <tr>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                <label>Confirm password:</label>
                                            </>
                                        ) : (
                                            <></>
                                        )}
                                    </td>
                                    <td>
                                        
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        {isEditing ? (
                                            <>
                                                <input
                                                    type="password"
                                                    className="form-control"
                                                    id="confirmation"
                                                    value={confirmation}
                                                    onChange={handleConfitmationChange}
                                                />
                                                <div className="error">{validationErrors.confirmation}</div>
                                            </>
                                        ) : (
                                            <></>
                                        )}
                                    </td>
                                    <td>
                                        
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        {isEditing ? (
                            <div className="edit-button-container">
                                <button className="btn btn-primary" onClick={handleSaveClick}>Save</button>
                                <button id="cancelBtn" className="btn btn-primary" onClick={handleCancelClick}>Cancel</button>
                            </div>
                        ) : (
                            <div className="edit-button-container">
                                <button className="btn btn-primary" onClick={handleEditClick}>Edit</button>
                            </div>
                        )}
                    </div>
                </>
            ) : (
                    <div className="alert alert-danger" style={{ marginTop: "20px" }} role="alert">
                    {alertMessage}
                </div>
            )}
        </div>
    );
};

export default ProfilePage;