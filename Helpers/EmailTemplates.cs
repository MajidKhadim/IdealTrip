namespace IdealTrip.Helpers
{
	public static class EmailTemplates
	{
		public static string ForgetPasswordEmailTemplate(string userFullName, string resetLink)
		{
			return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            padding: 20px;
            border: 1px solid #dddddd;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            text-align: center;
            background-color: #0078d7;
            color: #ffffff;
            padding: 15px 10px;
            border-radius: 8px 8px 0 0;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 20px;
        }}
        .email-content {{
            padding: 20px;
            color: #333333;
            font-size: 15px;
        }}
        .email-footer {{
            font-size: 12px;
            color: #777777;
            margin-top: 20px;
            padding-top: 10px;
            border-top: 1px solid #dddddd;
            text-align: left;
        }}
        .email-regards {{
            margin-top: 20px;
            font-size: 14px;
            font-weight: bold;
            color: #333333;
            text-align: left;
        }}
        .reset-link {{
            color: #0078d7;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='email-content'>
            <p>Hello {userFullName},</p>
            <p>You requested a password reset for your account. Click the link below to reset your password:</p>
            <p><a href='{resetLink}' class='reset-link'>Reset Your Password</a></p>
            <p>If you did not request this, please ignore this email. This link will expire in 5 minutes.</p>
        </div>
        <div class='email-regards'>
            <p>Regards,</p>
            <p><strong>Ideal Trip Team</strong></p>
            <p>Make your journey with pleasure😊😊!</p>
        </div>
        <div class='email-footer'>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";
		}

		public static string EmailVerificationTemplate(string userFullName, string verificationLink)
		{
			return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f9f9f9;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background: #ffffff;
            padding: 20px;
            border: 1px solid #dddddd;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            text-align: center;
            background-color: #0078d7;
            color: #ffffff;
            padding: 15px 10px;
            border-radius: 8px 8px 0 0;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 20px;
        }}
        .email-content {{
            padding: 20px;
            color: #333333;
            font-size: 15px;
        }}
        .email-footer {{
            font-size: 12px;
            color: #777777;
            margin-top: 20px;
            padding-top: 10px;
            border-top: 1px solid #dddddd;
            text-align: left;
        }}
        .email-regards {{
            margin-top: 20px;
            font-size: 14px;
            font-weight: bold;
            color: #333333;
            text-align: left;
        }}
        .verification-link {{
            color: #0078d7;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Email Verification</h1>
        </div>
        <div class='email-content'>
            <p>Hello {userFullName},</p>
            <p>Welcome to IdealTrip! To verify your email address, click the link below:</p>
            <p><a href='{verificationLink}' class='verification-link'>Verify Your Email</a></p>
            <p>If you did not sign up for this account, please ignore this email.</p>
        </div>
        <div class='email-regards'>
            <p>Regards,</p>
            <p><strong>Ideal Trip Team</strong></p>
            <p>Make your journey with pleasure😊😊!</p>
        </div>
        <div class='email-footer'>
            <p>This is an automated email. Please do not reply to this message.</p>
        </div>
    </div>
</body>
</html>";
		}
	}
}