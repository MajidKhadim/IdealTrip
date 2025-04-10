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
		public static string AccountApprovedTemplate(string userFullName, string loginLink)
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
        .login-link {{
            color: #0078d7;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Your Account Has Been Approved!</h1>
        </div>
        <div class='email-content'>
            <p>Hello {userFullName},</p>
            <p>We are excited to inform you that your account has been approved by our admin team! You can now log in and start using the IdealTrip application.</p>
            <p><a href='{loginLink}' class='login-link'>Login to Your Account</a></p>
            <p>If you have any issues, please feel free to contact us.</p>
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
		public static string AccountRejectedTemplate(string userFullName, string registerLink)
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
            background-color: #e74c3c;
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
        .register-link {{
            color: #e74c3c;
            text-decoration: none;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Account Rejected</h1>
        </div>
        <div class='email-content'>
            <p>Hello {userFullName},</p>
            <p>We regret to inform you that your account has been rejected. We encourage you to try registering again to join the IdealTrip platform.</p>
            <p><a href='{registerLink}' class='register-link'>Register Again</a></p>
            <p>If you believe this is a mistake or if you have any questions, please contact us.</p>
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

		public static string PaymentSuccessTemplate(string userFullName, string paymentAmount, string paymentDate, string productName, string productDetails, string paymentStatus, string transactionId)
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
            background-color: #4CAF50;
            color: #ffffff;
            padding: 15px 10px;
            border-radius: 8px 8px 0 0;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 22px;
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
        .product-details {{
            margin-top: 20px;
            padding: 10px;
            border: 1px solid #eeeeee;
            border-radius: 5px;
            background-color: #f1f1f1;
        }}
        .product-details h3 {{
            margin: 0;
            font-size: 18px;
        }}
        .transaction-info {{
            margin-top: 20px;
            padding: 10px;
            font-size: 14px;
            background-color: #f7f7f7;
            border: 1px solid #dddddd;
            border-radius: 5px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>Payment Success - Thank You for Your Payment!</h1>
        </div>
        <div class='email-content'>
            <p>Hello {userFullName},</p>
            <p>We are pleased to inform you that your payment has been successfully processed. Below are the details of your transaction:</p>

            <div class='product-details'>
                <h3>Product Details:</h3>
                <p><strong>Product Name:</strong> {productName}</p>
                <p><strong>Details:</strong> {productDetails}</p>
            </div>

            <div class='transaction-info'>
                <p><strong>Amount Paid:</strong> {paymentAmount}RS.</p>
                <p><strong>Payment Date:</strong> {paymentDate}</p>
                <p><strong>Status:</strong> {paymentStatus}</p>
                <p><strong>Transaction ID:</strong> {transactionId}</p>
            </div>

            <p>If you have any questions or need further assistance, feel free to contact us. We look forward to serving you on your upcoming journey!</p>
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
		public static string HotelBookingSuccessTemplate(
	string touristName,
	string touristEmail,
	string hotelName,
	string roomType,
	decimal totalAmount,
	string paymentId,
	string checkInDate,
	string checkOutDate,
	string bookingStatus,
    DateTime bookingTime
)
		{
			return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: 'Segoe UI', sans-serif;
                background-color: #f4f4f4;
                padding: 20px;
                color: #333;
            }}
            .email-container {{
                max-width: 600px;
                margin: auto;
                background: #ffffff;
                padding: 30px;
                border-radius: 10px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            }}
            h2 {{
                color: #2c3e50;
            }}
            .booking-details {{
                margin-top: 20px;
            }}
            .booking-details p {{
                margin: 10px 0;
                line-height: 1.6;
            }}
            .footer {{
                margin-top: 30px;
                font-size: 0.9em;
                color: #888;
                text-align: center;
            }}
        </style>
    </head>
    <body>
        <div class='email-container'>
            <h2>🎉 Hotel Booking Confirmed</h2>
            <p>Hi {touristName},</p>
            <p>We're thrilled to let you know that your hotel booking has been successfully confirmed. Below are your booking details:</p>

            <div class='booking-details'>
                <p><strong>Hotel Name:</strong> {hotelName}</p>
                <p><strong>Room Type:</strong> {roomType}</p>
                <p><strong>Check-In:</strong> {checkInDate}</p>
                <p><strong>Check-Out:</strong> {checkOutDate}</p>
                <p><strong>Total Amount Paid:</strong> ₹{totalAmount}</p>
                <p><strong>Payment ID:</strong> {paymentId}</p>
                <p><strong>Status:</strong> {bookingStatus}</p>
                <p><strong>Booking Time:</strong> {bookingTime}</p>
            </div>

            <p>If you have any questions or changes to your reservation, feel free to contact us at <a href='mailto:support@idealtrip.com'>support@idealtrip.com</a>.</p>

            <p>Thank you for choosing Ideal Trip. We wish you a comfortable and enjoyable stay!</p>

            <div class='footer'>
                &copy; {DateTime.Now.Year} Ideal Trip. All rights reserved.
            </div>
        </div>
    </body>
    </html>
    ";
		}
		public static string TourGuideBookingSuccessTemplate(
	string touristName,
	string tourGuideName,
	string tourGuideBio,
	string startDate,
	string endDate,
	int numberOfTravelers,
	decimal totalAmount,
	string paymentId,
	int totalDays,
	string status,
    DateTime bookingTime
)
		{
			return $@"
    <html>
    <head>
        <style>
            body {{
                font-family: 'Segoe UI', sans-serif;
                background-color: #f4f4f4;
                padding: 20px;
                color: #333;
            }}
            .email-container {{
                max-width: 600px;
                margin: auto;
                background: #ffffff;
                padding: 30px;
                border-radius: 10px;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            }}
            h2 {{
                color: #2c3e50;
            }}
            .booking-details {{
                margin-top: 20px;
            }}
            .booking-details p {{
                margin: 10px 0;
                line-height: 1.6;
            }}
            .footer {{
                margin-top: 30px;
                font-size: 0.9em;
                color: #888;
                text-align: center;
            }}
        </style>
    </head>
    <body>
        <div class='email-container'>
            <h2>🎉 Tour Guide Booking Confirmed</h2>
            <p>Hi {touristName},</p>
            <p>Thank you for booking your tour with <strong>{tourGuideName}</strong>. Your booking has been successfully confirmed! 🎒</p>

            <div class='booking-details'>
                <p><strong>Tour Guide:</strong> {tourGuideName}</p>
                <p><strong>Guide Bio:</strong> {tourGuideBio}</p>
                <p><strong>Start Date:</strong> {startDate}</p>
                <p><strong>End Date:</strong> {endDate}</p>
                <p><strong>Total Days:</strong> {totalDays}</p>
                <p><strong>Number of Travelers:</strong> {numberOfTravelers}</p>
                <p><strong>Total Amount Paid:</strong> ₹{totalAmount}</p>
                <p><strong>Payment ID:</strong> {paymentId}</p>
                <p><strong>Status:</strong> {status}</p>
                <p><strong>Booking Time: </strong>{bookingTime}</p>
            </div>

            <p>If you have any questions or special requests, feel free to reach out to your guide or contact us at <a href='mailto:support@idealtrip.com'>support@idealtrip.com</a>.</p>

            <p>We hope you have an amazing and unforgettable journey!</p>

            <div class='footer'>
                &copy; {DateTime.Now.Year} Ideal Trip. All rights reserved.
            </div>
        </div>
    </body>
    </html>
    ";
		}
		public static string LocalHomeBookingSuccessTemplate(
	string userName,
	string homeName,
	string homeDescription,
	DateOnly checkInDate,
	DateOnly checkOutDate,
	int totalNights,
	string totalAmount,
	string paymentIntentId,
	string status,
    DateTime bookingTime
)
		{
			return $@"
		<h2>🏠 Local Home Booking Confirmation</h2>
		<p>Hi <strong>{userName}</strong>,</p>
		<p>We’re excited to let you know that your local home booking has been <strong>successfully confirmed</strong>!</p>
		
		<h3>🏡 Home Details:</h3>
		<ul>
			<li><strong>Name:</strong> {homeName}</li>
			<li><strong>Description:</strong> {homeDescription}</li>
			<li><strong>Check-In:</strong> {checkInDate:MMMM dd, yyyy}</li>
			<li><strong>Check-Out:</strong> {checkOutDate:MMMM dd, yyyy}</li>
			<li><strong>Total Nights:</strong> {totalNights}</li>
			<li><strong>Total Amount Paid:</strong> ₹{totalAmount}</li>
			<li><strong>Payment ID:</strong> {paymentIntentId}</li>
			<li><strong>Status:</strong> {status}</li>
            <li><strong>Booking Time:</strong> {bookingTime}</li>
		</ul>

		<p>🛏 Your stay is reserved and ready to welcome you!</p>
		<p>If you have any questions or need assistance, feel free to contact us anytime.</p>
		<p>Thank you for choosing <strong>IdealTrip</strong> — we wish you a wonderful stay!</p>
	";
		}
		public static string TransportBookingSuccessTemplate(
	string userName,
	string transportName,
	string transportType,
	string from,
	string to,
	DateTime departureTime,
	DateTime bookingDate,
	int seatsBooked,
	string totalAmount,
	string paymentIntentId,
	string status
)
		{
			string emoji = transportType.ToLower() == "bus" ? "🚌" : "🚗";

			return $@"
		<h2>{emoji} Transport Booking Confirmation</h2>
		<p>Hi <strong>{userName}</strong>,</p>
		<p>We’re happy to confirm your <strong>{transportType}</strong> transport booking! Here are your trip details:</p>

		<h3>🛣 Journey Details:</h3>
		<ul>
			<li><strong>Transport:</strong> {transportName}</li>
			<li><strong>Type:</strong> {transportType}</li>
			<li><strong>From:</strong> {from}</li>
			<li><strong>To:</strong> {to}</li>
			<li><strong>Departure Time:</strong> {departureTime:MMMM dd, yyyy | hh:mm tt}</li>
			<li><strong>Booking Date:</strong> {bookingDate:MMMM dd, yyyy}</li>
			<li><strong>Seats Booked:</strong> {seatsBooked}</li>
			<li><strong>Total Fare:</strong> ₹{totalAmount}</li>
			<li><strong>Payment ID:</strong> {paymentIntentId}</li>
			<li><strong>Status:</strong> {status}</li>
		</ul>

		<p>🎫 Your seats are reserved and confirmed. Please be at the pickup point at least 15 minutes before departure.</p>
		<p>Thank you for choosing <strong>IdealTrip</strong> for your journey. Have a safe and pleasant ride!</p>
	";
		}





	}
}