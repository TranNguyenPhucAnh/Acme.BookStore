<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <style type="text/css">
        /* Reset styles để đảm bảo hiển thị nhất quán trên các email client */
        body, table, td, a { -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
        table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
        img { -ms-interpolation-mode: bicubic; border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }
        body { margin: 0; padding: 0; width: 100% !important; background-color: #f4f4f4; font-family: 'Helvetica Neue', Arial, sans-serif; }

        /* Container chính */
        .container { width: 100%; max-width: 600px; margin: 0 auto; background-color: #ffffff; border: 1px solid #e0e0e0; }
        .header { background-color: #007bff; padding: 20px; text-align: center; }
        .header img { max-width: 150px; height: auto; }
        .content { padding: 30px 20px; font-size: 16px; line-height: 1.6; color: #333333; }
        .footer { background-color: #f8f8f8; padding: 20px; text-align: center; font-size: 12px; color: #777777; }
        .footer a { color: #007bff; text-decoration: none; }
        .button { display: inline-block; padding: 12px 24px; background-color: #007bff; color: #ffffff !important; text-decoration: none; border-radius: 4px; font-size: 16px; margin: 10px 0; }

        /* Responsive design */
        @media screen and (max-width: 600px) {
            .container { width: 100% !important; }
            .header img { max-width: 120px; }
            .content { padding: 20px 10px; }
        }
    </style>
</head>
<body>
    <table width="100%" border="0" cellspacing="0" cellpadding="0" bgcolor="#f4f4f4">
        <tr>
            <td align="center" valign="top">
                <!-- Container chính -->
                <table class="container" border="0" cellspacing="0" cellpadding="0">
                    <!-- Header -->
                    <tr>
                        <td class="header">
                            <img src="https://d58j1vxq1cgpe.cloudfront.net/logo-light.png"
                                alt="Company Logo" />
                        </td>
                    </tr>
                    <!-- Nội dung động -->
                    <tr>
                        <td class="content">
                            {{content}}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td class="footer">
                            <p>&copy; {{model.year}} Book Store Demo. All rights reserved.</p>
                            <p>
                                <a href="https://bookstore-demo.online">Visit my demo</a> | 
                                <a href="mailto:phucanhatt@gmail.com">Contact Me</a> | 
                            </p>
                            <p>Ho Chi Minh City, Vietnam</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
