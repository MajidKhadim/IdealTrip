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
		public static string TourGuideBookingNotificationTemplate(
			string tourGuideName,
			string touristName,
			string touristEmail,
			string touristPhone,
			DateTime startDate,
			DateTime endDate,
			int numberOfTravelers,
			decimal totalAmount,
			string paymentIntentId,
			int totalDays,
			DateTime bookingDate,
            string SpecialRequest
		)
		{
			return $@"
		<!DOCTYPE html>
		<html>
		<head>
			<meta charset='UTF-8'>
			<title>New Booking Notification</title>
			<style>
				body {{
					font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
					background-color: #f9f9f9;
					color: #333;
					line-height: 1.6;
				}}
				.container {{
					width: 90%;
					max-width: 600px;
					margin: 0 auto;
					padding: 20px;
					background-color: #ffffff;
					border-radius: 8px;
					box-shadow: 0 2px 8px rgba(0,0,0,0.1);
				}}
				.header {{
					text-align: center;
					color: #2a9d8f;
					margin-bottom: 20px;
				}}
				.details {{
					margin-top: 20px;
				}}
				.footer {{
					margin-top: 30px;
					font-size: 14px;
					color: #777;
					text-align: center;
				}}
			</style>
		</head>
		<body>
			<div class='container'>
				<h2 class='header'>📢 New Tour Booking Received!</h2>
				<p>Dear <strong>{tourGuideName}</strong>,</p>

				<p>We are excited to inform you that a tourist has just confirmed a booking with you.</p>

				<div class='details'>
					<p><strong>Tourist Name:</strong> {touristName}</p>
					<p><strong>Email:</strong> {touristEmail}</p>
					<p><strong>Phone:</strong> {touristPhone}</p>
					<p><strong>Start Date:</strong> {startDate:dd MMM yyyy}</p>
					<p><strong>End Date:</strong> {endDate:dd MMM yyyy}</p>
					<p><strong>Total Days:</strong> {totalDays}</p>
					<p><strong>Number of Travelers:</strong> {numberOfTravelers}</p>
                    <p><strong>Special Request:</strong> {SpecialRequest}</p>
					<p><strong>Total Amount:</strong> ₹{totalAmount}</p>
					<p><strong>Payment ID:</strong> {paymentIntentId}</p>
					<p><strong>Booking Date:</strong> {bookingDate:dd MMM yyyy, hh:mm tt}</p>
				</div>

				<p>Please prepare accordingly to provide the best experience for your guest!</p>

				<div class='footer'>
					<p>— The Ideal Trip Team</p>
				</div>
			</div>
		</body>
		</html>";
		}
		public static string LocalHomeBookingOwnerNotificationTemplate(
	string ownerName,
	string touristName,
	string homeName,
	DateOnly startDate,
	DateOnly endDate,
	int totalDays,
	string amount,
	string paymentIntentId,
	DateTime bookingDate)
		{
			return $@"
		<div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
			<div style='text-align: center;'>
				<img src='https://i.imgur.com/NhQZ5uX.png' alt='Booking Confirmed' style='width: 100%; max-height: 200px; object-fit: cover; border-radius: 10px;' />
				<h2 style='color: #2c3e50;'>Your Local Home Has Been Booked! 🎉</h2>
			</div>
			<p>Hi <strong>{ownerName}</strong>,</p>
			<p>
				Your local home <strong style='color:#2980b9'>{homeName}</strong> has just been booked by <strong>{touristName}</strong>.
				Here's a summary of the booking:
			</p>
			<table style='width: 100%; border-collapse: collapse; margin-top: 15px;'>
				<tr>
					<td style='padding: 8px; font-weight: bold;'>📅 Booking Date:</td>
					<td style='padding: 8px;'>{bookingDate:MMMM dd, yyyy}</td>
				</tr>
				<tr>
					<td style='padding: 8px; font-weight: bold;'>🧳 Guest Name:</td>
					<td style='padding: 8px;'>{touristName}</td>
				</tr>
				<tr>
					<td style='padding: 8px; font-weight: bold;'>🏠 Stay Duration:</td>
					<td style='padding: 8px;'>{startDate:MMMM dd} - {endDate:MMMM dd} ({totalDays} nights)</td>
				</tr>
				<tr>
					<td style='padding: 8px; font-weight: bold;'>💰 Total Amount:</td>
					<td style='padding: 8px;'>₹{amount}</td>
				</tr>
				<tr>
					<td style='padding: 8px; font-weight: bold;'>🔐 Payment ID:</td>
					<td style='padding: 8px;'>{paymentIntentId}</td>
				</tr>
			</table>

			<p style='margin-top: 20px;'>Please make sure the property is ready and contactable before the guest's arrival. 🙌</p>

			<hr style='margin: 30px 0;' />

			<p style='font-size: 14px; color: #7f8c8d;'>
				Need help? Contact our support team anytime.<br />
				Thanks for being a valued part of <strong>Ideal Trip</strong>.
			</p>
		</div>
	";
		}

		public static string TransportBookingOwnerNotificationTemplate(
	string ownerName,
	string touristName,
	string touristEmail,
	string transportName,
	string transportType,
	string startLocation,
	string destination,
	DateTime departureTime,
	DateTime bookingDate,
	int seatsBooked,
	string totalFare,
	string paymentIntentId)
		{
			return $@"
		<div style='font-family: Arial, sans-serif; max-width: 650px; margin: auto; border: 1px solid #e5e5e5; border-radius: 12px; overflow: hidden;'>
			<div style='background-color: #2980b9; color: white; padding: 20px; text-align: center;'>
				<h2 style='margin: 0;'>🚍 Ideal Trip – New Transport Booking</h2>
			</div>
			<div style='padding: 20px; background-color: #f9f9f9;'>
				<p>Hi <strong>{ownerName}</strong>,</p>
				<p>Good news! Your <strong>{transportType}</strong> <span style='color: #2980b9;'>{transportName}</span> has just been booked by <strong>{touristName}</strong>.</p>

				<div style='background-color: white; border: 1px solid #ddd; border-radius: 10px; padding: 15px; margin-top: 20px;'>
					<table style='width: 100%; font-size: 15px;'>
						<tr>
							<td><strong>📅 Booking Date:</strong></td>
							<td>{bookingDate:MMMM dd, yyyy}</td>
						</tr>
						<tr>
							<td><strong>🛣️ Route:</strong></td>
							<td>{startLocation} → {destination}</td>
						</tr>
						<tr>
							<td><strong>🕒 Departure:</strong></td>
							<td>{departureTime:hh:mm tt}</td>
						</tr>
						<tr>
							<td><strong>🪑 Seats Booked:</strong></td>
							<td>{seatsBooked}</td>
						</tr>
						<tr>
							<td><strong>💰 Total Fare:</strong></td>
							<td>₹{totalFare}</td>
						</tr>
						<tr>
							<td><strong>🔐 Payment ID:</strong></td>
							<td>{paymentIntentId}</td>
						</tr>
					</table>
				</div>

				<p style='margin-top: 20px;'>Need to contact the passenger? Use the button below:</p>
				<div style='text-align: center; margin-top: 10px;'>
					<a href='mailto:{touristEmail}' style='background-color: #27ae60; color: white; text-decoration: none; padding: 12px 20px; border-radius: 8px; display: inline-block; font-weight: bold;'>📧 Contact {touristName}</a>
				</div>

				<p style='margin-top: 30px;'>Thank you for providing a reliable transport experience on <strong>Ideal Trip</strong>. Keep up the great service! 🌟</p>
			</div>
			<div style='background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 13px; color: #7f8c8d;'>
				Need help? Reach us at <a href='mailto:support@idealtrip.com'>support@idealtrip.com</a><br />
				&copy; {DateTime.Now.Year} Ideal Trip. All rights reserved.
			</div>
		</div>";
		}
		public static string HotelOwnerBookingNotificationTemplate(
			string ownerName,
			string guestName,
			string guestEmail,
			string hotelName,
			string roomType,
			DateOnly checkIn,
			DateOnly checkOut,
			string paymentIntentId,
			decimal totalAmount,
			DateTime bookingDate)
		{
			return $@"
		<div style='font-family: Arial, sans-serif; max-width: 650px; margin: auto; border: 1px solid #e5e5e5; border-radius: 12px; overflow: hidden;'>
			<div style='background-color: #2c3e50; color: white; padding: 20px; text-align: center;'>
				<h2 style='margin: 0;'>🏨 Ideal Trip – New Room Booking</h2>
			</div>
			<div style='padding: 20px; background-color: #f9f9f9;'>
				<p>Hi <strong>{ownerName}</strong>,</p>
				<p>Your hotel <strong>{hotelName}</strong> has just received a new room booking from <strong>{guestName}</strong>.</p>

				<div style='background-color: white; border: 1px solid #ddd; border-radius: 10px; padding: 15px; margin-top: 20px;'>
					<table style='width: 100%; font-size: 15px;'>
						<tr>
							<td><strong>🏨 Hotel Name:</strong></td>
							<td>{hotelName}</td>
						</tr>
						<tr>
							<td><strong>🛏️ Room Type:</strong></td>
							<td>{roomType}</td>
						</tr>
						<tr>
							<td><strong>📅 Check-In:</strong></td>
							<td>{checkIn:dd MMM yyyy}</td>
						</tr>
						<tr>
							<td><strong>📅 Check-Out:</strong></td>
							<td>{checkOut:dd MMM yyyy}</td>
						</tr>
						<tr>
							<td><strong>💳 Payment ID:</strong></td>
							<td>{paymentIntentId}</td>
						</tr>
						<tr>
							<td><strong>💰 Total Paid:</strong></td>
							<td>₹{totalAmount:F2}</td>
						</tr>
						<tr>
							<td><strong>📆 Booking Date:</strong></td>
							<td>{bookingDate:dd MMM yyyy, hh:mm tt}</td>
						</tr>
					</table>
				</div>

				<p style='margin-top: 20px;'>Need to contact the guest? Click below:</p>
				<div style='text-align: center; margin-top: 10px;'>
					<a href='mailto:{guestEmail}' style='background-color: #2980b9; color: white; text-decoration: none; padding: 12px 20px; border-radius: 8px; display: inline-block; font-weight: bold;'>📧 Contact {guestName}</a>
				</div>

				<p style='margin-top: 30px;'>Thanks for hosting with <strong>Ideal Trip</strong> – where great stays begin! 🌍</p>
			</div>
			<div style='background-color: #ecf0f1; padding: 15px; text-align: center; font-size: 13px; color: #7f8c8d;'>
				Need help? Email <a href='mailto:support@idealtrip.com'>support@idealtrip.com</a><br />
				&copy; {DateTime.Now.Year} Ideal Trip. All rights reserved.
			</div>
		</div>";
		}






	}
}