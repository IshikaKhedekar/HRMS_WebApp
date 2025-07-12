<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="HRMS_WebApp.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Indus Analytics HRMS</title>
    <link rel="stylesheet" href="https://cdn3.devexpress.com/jslib/23.1.6/css/dx.light.css" />
    <script src="https://ajax.aspnetcdn.com/ajax/jquery/jquery-3.5.1.min.js"></script>
    <script src="https://cdn3.devexpress.com/jslib/23.1.6/js/dx.all.js"></script>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />

    <style>
        * {
            box-sizing: border-box;
        }

        body, html {
            margin: 0;
            padding: 0;
            height: 100%;
            font-family: 'Poppins', sans-serif;
        }

        .main-container {
            display: flex;
            height: 100vh;
        }

        /* LEFT SIDE */
        .left-side {
            flex: 1;
            /* --- FIXED: Separated background shorthand properties --- */
            background-image: url('https://images.unsplash.com/photo-1519389950473-47ba0277781c');
            background-position: center;
            background-size: cover; /* 'cover' is a valid value for background-size */
            background-repeat: no-repeat;
            /* --- END FIX --- */
            position: relative;
            color: white;
            padding: 50px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            animation: fadeInLeft 1.2s ease-out;
        }

        .left-side::before {
            content: '';
            position: absolute;
            top: 0; left: 0; right: 0; bottom: 0;
            background: rgba(0, 0, 0, 0.5);
        }

        .left-content {
            position: relative;
            z-index: 1;
        }

        .left-content h1 {
            font-size: 40px;
            margin-bottom: 20px;
        }

        .left-content p {
            font-size: 18px;
            max-width: 400px;
            line-height: 1.6;
        }

        /* RIGHT SIDE */
        .right-side {
            flex: 1;
            background: #f4f8fb;
            display: flex;
            justify-content: center;
            align-items: center;
            animation: fadeInRight 1.2s ease-out;
        }

        .login-box {
            background: #fff;
            padding: 40px 30px;
            border-radius: 15px;
            box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 400px;
            animation: slideUp 1.2s ease-out;
        }

        .login-box h2 {
            text-align: center;
            margin-bottom: 25px;
            color: #333;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            display: block;
            margin-bottom: 6px;
            font-weight: 500;
        }

        .form-group input {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 14px;
        }

        .login-btn {
            background: #007bff;
            color: white;
            border: none;
            padding: 12px;
            border-radius: 6px;
            font-size: 16px;
            cursor: pointer;
            width: 100%;
            transition: background 0.3s ease;
        }

        .login-btn:hover {
            background: #0056b3;
        }

        .error-message {
            color: red;
            margin-top: 15px;
            font-weight: bold;
            text-align: center;
        }

        @keyframes fadeInLeft {
            from { opacity: 0; transform: translateX(-60px); }
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes fadeInRight {
            from { opacity: 0; transform: translateX(60px); }
            to { opacity: 1; transform: translateX(0); }
        }

        @keyframes slideUp {
            from { opacity: 0; transform: translateY(30px); }
            to { opacity: 1; transform: translateY(0); }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="main-container">
            <!-- Left Side -->
            <div class="left-side">
                <div class="left-content">
                    <h1>Indus Analytics</h1>
                    <p>Empowering HR through seamless <strong>Print Process Automation</strong>. Manage attendance, leaves, and payroll all in one place.</p>
                </div>
            </div>

            <!-- Right Side: Login Form -->
            <div class="right-side">
                <div class="login-box">
                    <h2>HRMS Login</h2>

                    <div class="form-group">
                        <label for="txtEmail">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                    </div>

                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
                    </div>

                    <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="login-btn" OnClick="btnLogin_Click" />

                    <div class="error-message">
                        <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>