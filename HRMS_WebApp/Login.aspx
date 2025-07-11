<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="HRMS_WebApp.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login - Indus Analytics HRM</title>

    <!-- DevExpress Styles & jQuery -->
    <link rel="stylesheet" type="text/css" href="https://cdn3.devexpress.com/jslib/23.1.6/css/dx.light.css" />
    <script type="text/javascript" src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.5.1.min.js"></script>
    <script type="text/javascript" src="https://cdn3.devexpress.com/jslib/23.1.6/js/dx.all.js"></script>

    <!-- Fonts & Styles -->
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        body {
            margin: 0;
            padding: 0;
            font-family: 'Poppins', sans-serif;
            background: linear-gradient(to right, #007bff, #00c6ff);
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            overflow: hidden;
        }

        /* Splash Screen */
        .splash {
            position: fixed;
            width: 100%;
            height: 100%;
            background: #fff;
            z-index: 9999;
            display: flex;
            justify-content: center;
            align-items: center;
            flex-direction: column;
            animation: fadeOut 2s ease-in-out 3s forwards;
        }

        .splash h1 {
            font-size: 3rem;
            color: #007bff;
            animation: slideIn 1s ease-out;
        }

        .splash p {
            font-size: 1.2rem;
            color: #333;
            margin-top: 10px;
            animation: slideIn 1.5s ease-out;
        }

        @keyframes fadeOut {
            to {
                opacity: 0;
                visibility: hidden;
            }
        }

        @keyframes slideIn {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Login Form Styling */
        .login-container {
            background-color: white;
            padding: 40px;
            border-radius: 12px;
            box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
            width: 350px;
            text-align: center;
            opacity: 0;
            transform: translateY(50px);
            animation: showLogin 1s ease-in-out 3.5s forwards;
        }

        @keyframes showLogin {
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .login-container h2 {
            margin-bottom: 25px;
            color: #333;
        }

        .form-group {
            margin-bottom: 20px;
            text-align: left;
        }

        .form-group label {
            font-weight: 500;
        }

        .error-message {
            color: #d9534f;
            margin-top: 15px;
            font-weight: bold;
        }
    </style>
</head>

<body>
    <!-- Splash Screen -->
    <div class="splash">
        <h1>Indus Analytics</h1>
        <p>Print Process Automation Partner</p>
    </div>

    <!-- Login Form -->
    <form id="form1" runat="server">
        <div class="login-container">
            <h2>HRM Portal Login</h2>

            <div class="form-group">
                <label>Email Address</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="dx-texteditor-input" Width="100%"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>Password</label>
                <asp:TextBox ID="txtPassword" runat="server" CssClass="dx-texteditor-input" TextMode="Password" Width="100%"></asp:TextBox>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="LOGIN" OnClick="btnLogin_Click"
                CssClass="dx-button dx-button-default dx-button-mode-contained" Width="100%" />

            <div class="error-message">
                <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
            </div>
        </div>
    </form>
</body>
</html>
