import "bootstrap/dist/css/bootstrap.min.css";
import React, { useState } from "react";
import ReCAPTCHA from "react-google-recaptcha";
import { Link, useNavigate } from "react-router-dom";
import 'react-toastify/dist/ReactToastify.css';
import { showFailedAlert, showSuccessAlert, showWarningAlert } from "../components/ToastifyAlert";

const LoginForm = ({ onLogin }) => {
  const [loginIdentifier, setLoginIdentifier] = useState("");
  const [password, setPassword] = useState("");
  const [authKey, setAuthKey] = useState(null);
  const [emailAuthorizationCode, setEmailAuthorizationCode] = useState(null);
  const [loginError, setLoginError] = useState("");
  const [visibleAuth, setVisibleAuth] = useState(false);
  const [visibleEmailAuthentication, setVisibleEmailAuthentication] = useState(false);
  const [recaptchaBool, setRecaptchaBool] = useState(null);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();

    const credentials = {
      login: loginIdentifier,
      password: password,
      authKey: authKey,
      emailAuthorizationCode: emailAuthorizationCode,
    };

    try {
      const response = await fetch("/api/account/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(credentials),
      });

        if (response.status === 202) {
            const data = await response.text();
            if (data == "Email Authentication") {
                setVisibleEmailAuthentication(true);
            }
            else {
                showWarningAlert("Two-Factor authentication!");
                setVisibleAuth(true);
            }
        }
      else if (response.ok) {
        showSuccessAlert("Login successful! ");
        onLogin({ username: loginIdentifier });
        navigate("/");
      }
      else if (visibleAuth) {
        setVisibleAuth(false);
          setAuthKey(null);
          showFailedAlert("Invalid Two-Factor code");
        setLoginError("Invalid Two-Factor code");
      }
      else {
          console.error("Login failed");
          showFailedAlert("Invalid username or password");
        setLoginError("Invalid username or password");
      }
    } catch (error) {
        console.error("Error during login", error);
        showFailedAlert("Error during login");
      setLoginError("Error during login");
    }
  };

  return (
    <div className="container">
          {!visibleAuth & !visibleEmailAuthentication ?
              <div>
                  <h2 className="mt-4">Login</h2>
                  <form onSubmit={handleLogin} className="row g-3">
                      <div className="col-md-6">
                          <label htmlFor="loginIdentifier" className="form-label">
                              Username or Email:
                          </label>
                          <input
                              type="text"
                              className="form-control"
                              id="loginIdentifier"
                              value={loginIdentifier}
                              onChange={(e) => setLoginIdentifier(e.target.value)}
                              required
                          />
                      </div>

                      <div className="col-md-6">
                          <label htmlFor="password" className="form-label">
                              Password:
                          </label>
                          <input
                              type="password"
                              className="form-control"
                              id="password"
                              value={password}
                              onChange={(e) => setPassword(e.target.value)}
                              required
                          />
                      </div>

                      {loginError && (
                          <div className="col-md-12 alert alert-danger">{loginError}</div>
                      )}

                      {loginError && (
                          <Link
                              to="/passwordRecovery"
                              className="text-decoration-none mt-0 text-danger"
                          >
                              Forgot password ?
                          </Link>
                      )}

                      <div className="col-12">
                          <ReCAPTCHA
                              sitekey="6LdmQ5QpAAAAANtlYo9h__kkYUPXSdysSOKIg16y"
                              onChange={(val) => setRecaptchaBool(val)}
                              className="mb-2"
                          />
                          <button disabled={!recaptchaBool} type="submit" className="btn btn-primary">
                              Login
                          </button>
                      </div>
                  </form>
              </div>
              : null}
          {visibleAuth ?
              <div className="">
                  <h2 className="mt-4">Two Factor Authentication</h2>
                  <form onSubmit={handleLogin} className="row g-3">
                      <div className="col-md-6">
                          <label htmlFor="authKey" className="form-label">
                              Code:
                          </label>
                          <input
                              type="number"
                              className="form-control"
                              id="authKey"
                              value={authKey}
                              onChange={(e) => setAuthKey(e.target.value)}
                              required
                          />
                      </div>
                      <div className="col-12">
                          <button type="submit" className="btn btn-primary">
                              Verify
                          </button>
                      </div>
                  </form>
              </div>
              : null}
          {visibleEmailAuthentication ?
              <div className="">
                  <h2 className="mt-4">Email Authentication</h2>
                  <form onSubmit={handleLogin} className="row g-3">
                      <div className="col-md-6">
                          <label htmlFor="emailAuthorizationCode" className="form-label">
                              Code:
                          </label>
                          <input
                              type="text"
                              className="form-control"
                              id="emailAuthorizationCode"
                              value={emailAuthorizationCode}
                              onChange={(e) => setEmailAuthorizationCode(e.target.value)}
                              required
                          />
                      </div>
                      <div className="col-12">
                          <button type="submit" className="btn btn-primary">
                              Verify
                          </button>
                      </div>
                  </form>
              </div>
              : null}
    </div>
  );
};

export default LoginForm;
