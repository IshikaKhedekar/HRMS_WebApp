<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="HRMS_WebApp.Login" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Indus Analytics HRMS</title>
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap" rel="stylesheet" />
    <style>
        * {
            box-sizing: border-box;
            font-family: 'Poppins', sans-serif;
        }

        html, body {
            margin: 0;
            padding: 0;
            height: 100%;
            background: linear-gradient(120deg, #0f2027, #203a43, #2c5364);
            overflow: hidden;
        }

        .background {
            position: absolute;
            top: 0; left: 0;
            width: 100%;
            height: 100%;
background-image: url('https://images.unsplash.com/photo-1581091870622-6c7a084a62db?auto=format&fit=crop&w=1500&q=80');
            background-size: cover;
            background-position: center;
            filter: brightness(0.5);
            z-index: -1;
            animation: zoomIn 8s ease-in-out infinite alternate;
        }

        .overlay {
            position: absolute;
            top: 0; left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.6);
            z-index: -1;
        }

        .content-container {
            height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            position: relative;
        }

        .login-wrapper {
            background: rgba(255, 255, 255, 0.12);
            border: 1px solid rgba(255, 255, 255, 0.2);
            backdrop-filter: blur(15px);
            padding: 50px 40px;
            border-radius: 20px;
            max-width: 900px;
            width: 100%;
            display: flex;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.3);
            animation: fadeIn 1.5s ease;
        }

        .company-info {
            flex: 1;
            padding-right: 30px;
            color: #ffffff;
            display: flex;
            flex-direction: column;
            justify-content: center;
            animation: slideInLeft 1s ease;
        }

        .company-info h1 {
            font-size: 42px;
            margin-bottom: 10px;
            color: #fff;
        }

        .company-info p {
            font-size: 16px;
            line-height: 1.6;
            color: #ddd;
        }

        .login-form {
            flex: 1;
            background: rgba(255, 255, 255, 0.9);
            padding: 35px;
            border-radius: 15px;
            animation: slideInRight 1s ease;
        }

        .login-form h2 {
            text-align: center;
            margin-bottom: 25px;
            font-weight: 600;
            color: #003366;
        }

        .form-group {
            margin-bottom: 20px;
        }

        .form-group label {
            font-weight: 500;
            display: block;
            margin-bottom: 6px;
            color: #333;
        }

        .form-group input {
            width: 100%;
            padding: 10px;
            border-radius: 8px;
            border: 1px solid #ccc;
            font-size: 15px;
        }

        .login-btn {
            width: 100%;
            background-color: #004e92;
            color: white;
            padding: 12px;
            border: none;
            font-size: 16px;
            border-radius: 8px;
            cursor: pointer;
            transition: background 0.3s ease;
        }

        .login-btn:hover {
            background-color: #002d5f;
        }

        .error-message {
            margin-top: 15px;
            color: red;
            text-align: center;
        }

        /* Animations */
        @keyframes slideInLeft {
            from { transform: translateX(-50px); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }

        @keyframes slideInRight {
            from { transform: translateX(50px); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }

        @keyframes zoomIn {
            0% { transform: scale(1); }
            100% { transform: scale(1.05); }
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: scale(0.95); }
            to { opacity: 1; transform: scale(1); }
        }

        @media (max-width: 768px) {
            .login-wrapper {
                flex-direction: column;
                padding: 30px;
            }

            .company-info {
                padding: 0 0 30px 0;
                text-align: center;
            }
        }
    </style>
</head>
<body>
    <div class="background"></div>
    <div class="overlay"></div>

    <form id="form1" runat="server">
        <div class="content-container">
            <div class="login-wrapper">
                <!-- Company Info -->
                <div class="company-info">
                    <h1>Indas Analytics</h1>
                    <p>Your Print Process Automation Partner Indus Analytics offers extended solutions to build a profitable and scalable printing and packaging company.</p>
                </div>

                <!-- Login Form -->
                <div class="login-form">
                    <h2>Sign In</h2>
                    <div class="form-group">
                        <label for="txtEmail">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
                    </div>
                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="login-btn" OnClick="btnLogin_Click" />
                    <div class="error-message">
                        <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
