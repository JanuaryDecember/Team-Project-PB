import React, { useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { useNavigate } from "react-router-dom";

const TwoFactorAuthentication = () => {
    const [TwoFactorStatus, setTwoFactorStatus] = useState(null);
    const [EmailAuthenticationStatus, setEmailAuthenticationStatus] = useState(null);
    const [googleAuthKey, setGoogleAuthKey] = useState('');
    const navigate = useNavigate();
    const [isLoggedIn, setIsLoggedIn] = useState(true);
    const [alertMessage, setAlertMessage] = useState("");
    const [emailAuthenticationAlertMessage, setEmailAuthenticationAlertMessage] = useState("");


    const activateTwoFactor = async (status) => {
        try {
            const enteredValue = document.getElementById('userAuthKey').value;

            const sendData = {
                authKey: googleAuthKey,
                enteredAuthKey: enteredValue
            };

            const response = await fetch('/api/account/enableTwoFactor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(sendData),
            });

            if (response.ok) {
                const data = await response.json();

                var displayDiv = document.createElement("div");
                displayDiv.setAttribute("role", "alert");

                if (data) {
                    setTwoFactorStatus(true);
                    updateStatusInfo(true);
                    displayDiv.classList.add("alert", "alert-success");
                    displayDiv.textContent = "Activated!";
                }
                else {
                    displayDiv.classList.add("alert", "alert-danger");
                    displayDiv.textContent = "Code is expired or wrong!";
                }
                var x = document.getElementById('activation-alert');
                x.innerHTML = "";
                x.appendChild(displayDiv);
            }
            else {
                throw new Error('Failed to update Two Factor Status');
            }
        } catch (error) {
            console.error('Error:', error);
        }
    };

    const disableTwoFactor = async (status) => {
        try {
            const enteredValue = document.getElementById('userAuthKey').value;

            const sendData = {
                enteredAuthKey: enteredValue
            };

            const response = await fetch('/api/account/disableTwoFactor', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(sendData),
            });

            if (response.ok) {
                const data = await response.json();

                var displayDiv = document.createElement("div");
                displayDiv.setAttribute("role", "alert");

                if (data) {
                    setTwoFactorStatus(false);
                    updateStatusInfo(false);
                    displayDiv.classList.add("alert", "alert-success");
                    displayDiv.textContent = "Disabled!";
                }
                else {
                    displayDiv.classList.add("alert", "alert-danger");
                    displayDiv.textContent = "Code is expired or wrong!";
                }
                var x = document.getElementById('activation-alert');
                x.innerHTML = "";

                x.appendChild(displayDiv);
                await fetchTwoFactorKey();
            }
            else {
                throw new Error('Failed to disable 2FA');
            }
        } catch (error) {
            console.error('Error:', error);
        }
    };

    const updateStatusInfo = (status) => {
        const spinner = document.querySelector('.status-info .spinner-border');
        if (status != null) {
            if (spinner)
                spinner.remove();

            if (status === true) {
                document.getElementsByClassName('status-info')[0].innerHTML = "<div class=\"h5 text-success\">Enabled</div>";
            }
            else if (status === false) {
                document.getElementsByClassName('status-info')[0].innerHTML = "<div class=\"h5 text-danger\">Disabled</div>";
            }
        }
    };

    const updateEmailAuthenticationStatus = (status) => {
        const spinner = document.querySelector('.status-info .spinner-border');
        if (status != null) {
            if (spinner)
                spinner.remove();

            if (status === true) {
                document.getElementsByClassName('email-authentication-status-info')[0].innerHTML = "<div class=\"h5 text-success\">Enabled</div>";
            }
            else if (status === false) {
                document.getElementsByClassName('email-authentication-status-info')[0].innerHTML = "<div class=\"h5 text-danger\">Disabled</div>";
            }
        }
    };

    const sendEmailAuthenticationCode = async () => {
        try {
            const response = await fetch('/api/account/sendEmailAuthenticationCode', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (response.ok) {
                const data = await response.json();
                setEmailAuthenticationAlertMessage(data.message);
                console.log(data.message);
            } else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    const enableEmailAuthentication = async () => {
        try {
            const response = await fetch('/api/account/enableEmailAuthentication', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (response.ok) {
                const data = await response.json();
                setEmailAuthenticationAlertMessage(data.message);

                setEmailAuthenticationStatus(true);
                updateEmailAuthenticationStatus(true);
            } else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    const disableEmailAuthentication = async () => {
        try {
            const emailAuthCode = document.getElementById('emailAuthCode').value;

            const data = {
                emailAuthenticationCode: emailAuthCode
            };

            const response = await fetch('/api/account/disableEmailAuthentication', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(data)
            });

            if (response.ok) {
                setEmailAuthenticationStatus(false);
                updateEmailAuthenticationStatus(false);
                const data = await response.json();
                setEmailAuthenticationAlertMessage(data.message);
            }
            else if(response.status === 401) {
                const data = await response.json();
                setEmailAuthenticationAlertMessage(data.message);
            }
            else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };



    const fetchTwoFactorStatus = async () => {
        try {
            const response = await fetch('/api/account/GetTwoFactorStatus', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (response.ok) {
                const data = await response.json();

                setTwoFactorStatus(data.twoFactorEnabled);
                updateStatusInfo(data.twoFactorEnabled);
                if (!data.twoFactorEnabled) {
                    await fetchTwoFactorKey();
                }
            } else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    const fetchTwoFactorKey = async () => {
        try {
            const response = await fetch('/api/account/GetTwoFactorKey', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (response.ok) {
                const data = await response.json();
                document.getElementById('QR_image').src = data.barcodeImageUrl;
                document.querySelector('.figure-caption').textContent = "Key: " + data.authKey;
                setGoogleAuthKey(data.authKey);
            } else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    useEffect(() => {
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
                    setAlertMessage("You are not logged in. Redirecting to login page...");
                }
            } catch (error) {
                console.error("Error checking user login:", error);
            }
        };
        checkUserLogin();
        fetchTwoFactorStatus();
        fetchEmailAuthenticationStatus();
    }, [navigate]);

    useEffect(() => {
        if (!isLoggedIn) {
            const timeout = setTimeout(() => {
                navigate("/");
            }, 3000);

            return () => clearTimeout(timeout);
        }
    }, [isLoggedIn, navigate]);

    const fetchEmailAuthenticationStatus = async () => {
        try {
            const response = await fetch('/api/account/getEmailAuthenticationStatus', {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
            });

            if (response.ok) {
                const data = await response.json();
                setEmailAuthenticationStatus(data.emailAuthentication);
                updateEmailAuthenticationStatus(data.emailAuthentication);
            } else {
                throw new Error('Failed to fetch data');
            }
        } catch (error) {
            console.error('Error fetching data:', error);
        }
    };

    return (
        <div className="container mt-5">
            <div className="row text-center">
                <h1>Two-Factor Authentication</h1>
            </div>  <hr></hr>


            <div className="row">
                <h2>Google Authenticator</h2>
            </div> <hr style={{ border: "1px dashed black" }}></hr>
            <div className="row mb-2">
                <div className="col h4">
                    Status
                </div>
                <div className="col-8 status-info">
                    <div className="spinner-border" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>
            </div> <hr style={{ border: "1px dashed black" }}></hr>
            {!TwoFactorStatus ? (
                <div className="row mb-2">
                    <div className="col-2">
                        <img src="" className="img-fluid" alt="QR Code" id="QR_image"></img>
                        <figcaption className="figure-caption text-center"></figcaption>
                    </div>
                    <div className="col-10 align-items-center">
                        <h4>How to Enable</h4>
                        <ol>
                            <li>Download and install the Google Authenticator app from the app store on your mobile device. This app will generate codes for your 2FA.</li>
                            <li>Open the Authenticator app and choose the option to add a new account. Use the app to scan the provided QR code or manually enter the secret key provided.</li>
                            <li>Once the QR code is scanned or the secret key is entered, the app will generate a unique code for your account. Enter this code on the website to confirm setup.</li>
                        </ol>
                    </div>
                </div>
            ) : (
                <div className="row mb-2">
                    <div>
                        <p>Your Two-Factor Authentication is currently active.</p>
                        <p>To disable 2FA, please follow these steps:</p>
                        <ol>
                            <li>Open your Google Authenticator app.</li>
                            <li>Enter the code generated by Google Authenticator below:</li>
                        </ol>
                    </div>
                </div>
            )}
            <div className="row mt-4">
                <div className="col h4">
                    <input type="number" className="form-control" placeholder="enter the digit code" id="userAuthKey" maxLength="6"></input>
                </div>
                <div className="col-8 status-info">
                    <input className="btn btn-dark" type="button" value={!TwoFactorStatus ? ("Enable") : ("Disable")} onClick={!TwoFactorStatus ? (activateTwoFactor) : (disableTwoFactor)}></input>
                </div>
            </div>
            <div className="row mt-4" id="activation-alert">
            </div>
            {!isLoggedIn && alertMessage !== "" && (
                <div className="alert alert-danger" role="alert">
                    {alertMessage}
                </div>
            )}
             <hr></hr>
            <div className="row">
                <h2>Email</h2>
            </div> <hr style={{ border: "1px dashed black" }}></hr>
            <div className="row mb-2">
                <div className="col h4">
                    Status
                </div>
                <div className="col-8 email-authentication-status-info">
                    <div className="spinner-border" role="status">
                        <span className="visually-hidden">Loading...</span>
                    </div>
                </div>
            </div> <hr style={{ border: "1px dashed black" }}></hr>

            {!EmailAuthenticationStatus ? (
                ""
            ) : (
                <div className="row mb-2">
                    <div>
                        <p>Your Two-Factor Authentication is currently active.</p>
                        <p>To disable, please follow these steps:</p>
                        <ol>
                            <li>Send email with code</li>
                            <li>Enter this code on the website to confirm</li>
                        </ol>
                    </div>
                </div>
            )}


            <div className="row mt-4">
                {EmailAuthenticationStatus ? (
                    <div className="col h4">
                        <input type="text" className="form-control" placeholder="enter code" id="emailAuthCode"></input>
                    </div>
                ) :
                    ("")
                }
                <div className="col-8 status-info">
                    <input className="btn btn-dark" type="button" value={!EmailAuthenticationStatus ? ("Enable") : ("Disable")} onClick={!EmailAuthenticationStatus ? (enableEmailAuthentication) : (disableEmailAuthentication)}></input>
                    <input className="btn btn-dark m-2" type="button" value="Send code" onClick={(sendEmailAuthenticationCode)}></input>
                </div>
            </div>
            <div className="row mt-4" id="activation-alert">
            </div>
            {emailAuthenticationAlertMessage !== "" && (
                <div className="alert alert-info" role="alert">
                    {emailAuthenticationAlertMessage}
                </div>
            )}

        </div>
    );
};

export default TwoFactorAuthentication;