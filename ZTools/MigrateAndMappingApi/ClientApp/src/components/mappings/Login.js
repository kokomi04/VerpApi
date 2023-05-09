import React, { useState } from "react";
import ApiEndPoint from "../../services/ApiEndpointService"
import { createBrowserHistory } from 'history';
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
    var configs = null;
    ApiEndPoint.getConfig().then(reponse => {
        configs = reponse.data;

    })
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
            const apiCore = ApiEndPoint.Login();
            var bodyFormData = new URLSearchParams();
            bodyFormData.append(
                "grant_type", "password"
            );
            bodyFormData.append("username", username);
            bodyFormData.append("password", password);
            bodyFormData.append("subsidiary_id", configs.Configs["subsidiary_id"]);
            bodyFormData.append("client_id", configs.Configs["client_id"]);
            bodyFormData.append("client_secret", configs.Configs["client_secret"]);
            const config = {
                headers: { 'content-type': 'application/x-www-form-urlencoded;charset=utf-8' }
            }
            apiCore.post(configs.Identity["Endpoint"], bodyFormData, config)
                .then(response => {
                    if (response.data ) {
                        setIsSubmitted(true);
                        localStorage.setItem("access_token", response.data["access_token"]);
                        localStorage.setItem("expires_at", JSON.stringify(response.data["expires_in"] * 1000 + new Date().getTime()));
                        //  token = response.data["access_token"];
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

    const goHome = () => {

        history.push('/');
        history.go(0);
    }

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

    return (
        <div className="app">
            <div className="login-form">
                <div className="title">Sign In</div>
                {isSubmitted ? <div>User is successfully logged in.</div> : renderForm}
            </div>
        </div>
    );
}
export default Login;
