# JWT Authentication Setup

## Overview

This API now validates JWT tokens issued by an external authentication service. The token is decoded and user claims (employee ID, role, email, name) are extracted to authorize requests.

---

## Configuration

### appsettings.json

```json
"Jwt": {
  "Issuer": "your-auth-service",
  "Audience": "hrms-api",
  "SecretKey": "your-secret-key-min-32-characters-long-replace-this"
}
```

**Important:** 
- Replace `Issuer` with the actual issuer from your auth service (e.g., `https://auth.yourcompany.com`)
- Replace `Audience` with the audience identifier for this API
- Replace `SecretKey` with the same secret key used by the auth service to sign tokens
- For production, use environment variables or secret managers (Azure Key Vault, AWS Secrets Manager, etc.)

---

## Expected JWT Claims

The API expects the following claims in the JWT token:

| Claim Name | Type | Description | Required |
|------------|------|-------------|----------|
| `employee_id` or `sub` | int | Employee ID in the system | ✅ Yes |
| `role` | string | User role (Employee, Manager, Admin) | Recommended |
| `email` | string | User email address | Optional |
| `name` | string | User full name | Optional |

### Example JWT Payload

```json
{
  "sub": "123",
  "employee_id": 123,
  "email": "john.doe@company.com",
  "name": "John Doe",
  "role": "Employee",
  "iss": "your-auth-service",
  "aud": "hrms-api",
  "exp": 1735689600,
  "iat": 1735603200
}
```

---

## How to Use

### Making Authenticated Requests

Include the JWT token in the `Authorization` header:

```bash
curl -X GET "http://localhost:5000/api/v1/requests" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Testing Locally

You can generate a test JWT token using online tools like [jwt.io](https://jwt.io) or create one programmatically:

```csharp
// Example: Generate test token (for development only)
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key-min-32-characters-long-replace-this"));
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim("employee_id", "1"),
    new Claim("role", "Employee"),
    new Claim("email", "test@company.com"),
    new Claim("name", "Test User")
};

var token = new JwtSecurityToken(
    issuer: "your-auth-service",
    audience: "hrms-api",
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: credentials
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
```

---

## Authorization Rules

### Endpoints Protected by [Authorize]

All endpoints in `RequestsController` and `AttendanceController` require authentication.

### Role-Based Access

- **Regular Employees:**
  - Can create, view, and cancel their own requests
  - Can check-in/check-out
  - Can view their own attendance history

- **Managers/Admins:**
  - Can view all requests and attendance records (via `employee_id` query parameter)
  - Can approve and reject requests
  - All employee capabilities

### Implementation

Controllers use the `ClaimsPrincipalExtensions` helper to extract user info:

```csharp
// Get employee ID from token
var employeeId = User.GetEmployeeId();

// Get user role
var role = User.GetRole();

// Check if user is manager or admin
if (User.IsManagerOrAdmin())
{
    // Manager/Admin logic
}
```

---

## Error Responses

### 401 Unauthorized
- Missing or invalid JWT token
- Token expired
- Token signature verification failed

```json
{
  "error": "Unauthorized",
  "message": "Authorization header is missing or invalid"
}
```

### 403 Forbidden
- Valid token but insufficient permissions (e.g., employee trying to approve requests)

```json
{
  "error": "Forbidden",
  "message": "Only managers or admins can approve requests"
}
```

---

## Integration with External Auth Service

This API validates tokens but does NOT issue them. Your external authentication service should:

1. **Issue JWT tokens** with the required claims (`employee_id`, `role`, etc.)
2. **Use the same secret key** for signing (configured in `Jwt:SecretKey`)
3. **Set correct issuer and audience** matching the configuration

### Auth Service Checklist

- [ ] JWT tokens include `employee_id` or `sub` claim
- [ ] JWT tokens include `role` claim
- [ ] Tokens are signed with the same secret key
- [ ] `iss` (issuer) matches `Jwt:Issuer` in appsettings
- [ ] `aud` (audience) matches `Jwt:Audience` in appsettings
- [ ] Token expiration (`exp`) is set appropriately

---

## Troubleshooting

### "Employee ID not found in token"
- Ensure the JWT contains `employee_id`, `sub`, or `NameIdentifier` claim
- Check claim names match what the auth service provides

### "IDX10501: Signature validation failed"
- Secret key mismatch between auth service and this API
- Verify `Jwt:SecretKey` matches the signing key

### "IDX10214: Audience validation failed"
- Audience in token doesn't match `Jwt:Audience`
- Update configuration or token generation

### "IDX10205: Issuer validation failed"
- Issuer in token doesn't match `Jwt:Issuer`
- Update configuration or token generation

---

## Security Best Practices

1. **Never commit secrets** - Use environment variables or secret managers
2. **Use HTTPS** - Always use HTTPS in production to protect tokens in transit
3. **Set appropriate token expiration** - Balance security vs user experience (typically 15-60 minutes)
4. **Rotate keys regularly** - Update secret keys periodically
5. **Validate all claims** - Don't trust token content without validation
6. **Monitor token usage** - Log authentication failures for security auditing

---

Last updated: December 8, 2025
