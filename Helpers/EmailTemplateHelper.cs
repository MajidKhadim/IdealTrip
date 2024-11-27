namespace IdealTrip.Helpers
{
	public static class EmailTemplateHelper
	{
		public static string GenerateEmailTemplate(string headerText, string bodyContent, string buttonText, string buttonLink)
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
            background-color: #000000;
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
        .email-content p {{
            margin: 10px 0;
        }}
        .email-content a {{
            display: inline-block;
            margin: 20px 0;
            padding: 12px 20px;
            background-color: #28a745;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
        .email-content a:hover {{
            background-color: #218838;
        }}
        .email-footer {{
            text-align: center;
            font-size: 12px;
            color: #777777;
            margin-top: 20px;
            padding-top: 10px;
            border-top: 1px solid #dddddd;
        }}
        .email-regards {{
            margin-top: 20px;
            padding: 10px;
            font-size: 14px;
            font-weight: bold;
            color: #333333;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='email-header'>
            <h1>{headerText}</h1>
        </div>
        <div class='email-content'>
            {bodyContent}
            <a href='{buttonLink}' target='_blank'>{buttonText}</a>
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
