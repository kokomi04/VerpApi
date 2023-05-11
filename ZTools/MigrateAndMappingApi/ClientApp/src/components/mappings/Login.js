import React, { useCallback, useEffect, useState } from "react";
import ApiEndPoint from "../../services/ApiEndpointService"
import { createBrowserHistory } from 'history';
import { useDispatch } from "react-redux";
import { configs } from '../../index'
function Login() {
    // React States
    const [errorMessages, setErrorMessages] = useState({});
    const [isSubmitted, setIsSubmitted] = useState(false);
    // User Login info
    const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
    var history = createBrowserHistory({ basename: baseUrl });
    const errors = {
        uname: "invalid username",
        pass: "invalid password"
    };

    const handleSubmit = (event) => {
        //Prevent page reload
        event.preventDefault();
        var { uname, pass } = document.forms[0];
        var username = uname.value;
        var password = pass.value;
        // Find user login info
        if (username == null || password == null) {
            // Invalid password
            setErrorMessages({ name: "pass", message: errors.pass });
        } else if (configs != null) {
            //setIsSubmitted(true);
            const apiCore = ApiEndPoint.getApiServiceCore();
            var bodyFormData = new URLSearchParams();
            bodyFormData.append(
                "grant_type", "password"
            );
            bodyFormData.append("username", username);
            bodyFormData.append("password", password);
            bodyFormData.append("subsidiary_id", configs.Identity["SubsidiaryId"]);
            bodyFormData.append("client_id", configs.Identity["ClientId"]);
            bodyFormData.append("client_secret", configs.Identity["ClientSecret"]);
            const config = {
                headers: { 'content-type': 'application/x-www-form-urlencoded;charset=utf-8' }
            }
            var a = configs.ServerURL;
            apiCore.post(configs.ServerURL, bodyFormData, config)
                .then(response => {
                    if (response.data) {
                        setIsSubmitted(true);
                        localStorage.setItem("access_token", response.data["access_token"]);
                        localStorage.setItem("expires_at", JSON.stringify(response.data["expires_in"] * 1000 + new Date().getTime()));
                        history.push('/');
                        history.go(0);
                    }

                }).catch(err =>
                    setErrorMessages({ name: "pass", message: errors.pass }));

        }
    };

    // Generate JSX code for error message
    const renderErrorMessage = (name) =>
        name === errorMessages.name && (
            <div className="error">{errorMessages.message}</div>
        );
    // JSX code for login form
    const renderForm = (
        <div className="form">
            <form onSubmit={handleSubmit}>
                <div className="input-container">
                    <label>Username </label>
                    <input type="text" name="uname" required />
                    {renderErrorMessage("uname")}
                </div>
                <div className="input-container">
                    <label>Password </label>
                    <input type="password" name="pass" required />
                    {renderErrorMessage("pass")}
                </div>
                <div className="button-container">
                    <input type="submit" />
                </div>
            </form>
        </div>
    );
    if (localStorage.getItem('access_token') == null) {
        return (
            <div className="app">
                <div className="login-form">
                    <div className="title">Sign In</div>
                    {isSubmitted ? <div>User is successfully logged in.</div> : renderForm}
                </div>
            </div>
        );
    } else {
        history.push('/');
        history.go(0);
    }

}
export default Login;
