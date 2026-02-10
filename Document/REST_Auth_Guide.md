# HƯỚNG DẪN SỬ DỤNG REST API AUTHENTICATION

## 📋 TỔNG QUAN

Hệ thống SmartService sử dụng **REST API** cho authentication với các tính năng:
- ✅ **Register** - Đăng ký tài khoản mới
- ✅ **Login** - Đăng nhập với email/password
- ✅ **RefreshToken** - Làm mới access token
- ✅ **Logout** - Đăng xuất (revoke refresh token)

**GraphQL chỉ dùng cho queries (read operations)** - khi cần authentication, gửi JWT token trong header.

### **Kiến trúc:**
- **Clean Architecture** - Tách biệt rõ ràng giữa các layers
- **CQRS Pattern** - Commands qua REST API, Queries qua GraphQL
- **MediatR** - Tất cả auth operations sử dụng Commands
- **Configuration-driven** - Token lifetimes được cấu hình trong `appsettings.json`

---

## 🔧 CẤU HÌNH

### **1. appsettings.json**

Token configuration được đặt trong `JwtSettings`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "SmartService",
    "Audience": "SmartServiceUsers",
    "AccessTokenLifetimeMinutes": 15,
    "RefreshTokenLifetimeDays": 7,
    "EncryptionKey": "Your32CharacterEncryptionKeyForAES!",
    "EncryptionIV": "Your16CharIVKey!"
  }
}
```

**Lưu ý:**
- `SecretKey`: Phải có ít nhất 32 ký tự (cho JWT signing)
- `EncryptionKey`: Phải có đúng 32 ký tự (cho AES encryption)
- `EncryptionIV`: Phải có đúng 16 ký tự (cho AES IV)
- Thay đổi các giá trị này trong production!

### **2. Database Migration**

Sau khi thêm `AuthData` entity, cần tạo migration:

```bash
dotnet ef migrations add AddAuthData --project SmartService.Infrastructure --startup-project SmartService.WebAPI
dotnet ef database update --project SmartService.Infrastructure --startup-project SmartService.WebAPI
```

---

## 🚀 SỬ DỤNG REST API AUTHENTICATION

### **Base URL:**
```
POST /api/auth
```

### **1. REGISTER - Đăng ký tài khoản**

**Endpoint:**
```
POST /api/auth/register
```

**Request Body:**
```json
{
  "email": "customer@example.com",
  "password": "SecurePassword123!",
  "fullName": "Nguyễn Văn A",
  "phoneNumber": "0901234567",
  "role": "CUSTOMER"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "raw_refresh_token_base64_string",
  "accessTokenExpiresAt": "2026-01-23T10:45:00Z",
  "refreshTokenExpiresAt": "2026-01-30T10:30:00Z",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "customer@example.com",
  "fullName": "Nguyễn Văn A",
  "role": "CUSTOMER"
}
```

**User Roles:**
- `CUSTOMER` - Khách hàng
- `STAFF` - Nhân viên quản lý
- `AGENT` - Nhà cung cấp dịch vụ
- `ADMIN` - Quản trị viên

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@example.com",
    "password": "SecurePassword123!",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0901234567",
    "role": "CUSTOMER"
  }'
```

---

### **2. LOGIN - Đăng nhập**

**Endpoint:**
```
POST /api/auth/login
```

**Request Body:**
```json
{
  "email": "customer@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "raw_refresh_token_base64_string",
  "accessTokenExpiresAt": "2026-01-23T10:45:00Z",
  "refreshTokenExpiresAt": "2026-01-30T10:30:00Z",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "customer@example.com",
  "fullName": "Nguyễn Văn A",
  "role": "CUSTOMER"
}
```

**Error Response (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "": ["Invalid email or password."]
  }
}
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@example.com",
    "password": "SecurePassword123!"
  }'
```

---

### **3. REFRESH TOKEN - Làm mới access token**

Khi access token hết hạn, sử dụng refresh token để lấy access token mới.

**Endpoint:**
```
POST /api/auth/refresh-token
```

**Request Body:**
```json
{
  "refreshToken": "raw_refresh_token_from_login_response"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_raw_refresh_token_base64_string",
  "accessTokenExpiresAt": "2026-01-23T11:00:00Z",
  "refreshTokenExpiresAt": "2026-01-30T10:30:00Z",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "email": "customer@example.com",
  "fullName": "Nguyễn Văn A",
  "role": "CUSTOMER"
}
```

**Lưu ý:** Refresh token cũ sẽ bị vô hiệu hóa, refresh token mới sẽ được trả về.

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/auth/refresh-token \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token_here"
  }'
```

---

### **4. LOGOUT - Đăng xuất**

Revoke refresh token để đăng xuất.

**Endpoint:**
```
POST /api/auth/logout
```

**Request Body:**
```json
{
  "refreshToken": "raw_refresh_token_to_revoke"
}
```

**Response (200 OK):**
```json
true
```

**cURL Example:**
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your_refresh_token_here"
  }'
```

---

## 🔐 SỬ DỤNG ACCESS TOKEN VỚI GRAPHQL QUERIES

Sau khi có `accessToken` từ REST API, thêm vào HTTP Header khi gọi GraphQL queries:

```
Authorization: Bearer {accessToken}
```

**Ví dụ với cURL:**
```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "query": "query { getUsers { id email fullName } }"
  }'
```

**Ví dụ với JavaScript (fetch):**
```javascript
const response = await fetch('http://localhost:5000/graphql', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${accessToken}`
  },
  body: JSON.stringify({
    query: `
      query {
        getUsers {
          id
          email
          fullName
        }
      }
    `
  })
});
```

---

## 📱 VÍ DỤ HOÀN CHỈNH (JavaScript/TypeScript)

### **Auth Service Class:**

```typescript
class AuthService {
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private baseUrl = 'http://localhost:5000/api/auth';
  private graphqlUrl = 'http://localhost:5000/graphql';

  async register(email: string, password: string, fullName: string, phoneNumber: string, role: string) {
    const response = await fetch(`${this.baseUrl}/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email,
        password,
        fullName,
        phoneNumber,
        role
      })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.title || 'Registration failed');
    }

    const result = await response.json();
    this.accessToken = result.accessToken;
    this.refreshToken = result.refreshToken;
    localStorage.setItem('accessToken', this.accessToken);
    localStorage.setItem('refreshToken', this.refreshToken);
    return result;
  }

  async login(email: string, password: string) {
    const response = await fetch(`${this.baseUrl}/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email,
        password
      })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.title || 'Login failed');
    }

    const result = await response.json();
    this.accessToken = result.accessToken;
    this.refreshToken = result.refreshToken;
    localStorage.setItem('accessToken', this.accessToken);
    localStorage.setItem('refreshToken', this.refreshToken);
    return result;
  }

  async refreshAccessToken() {
    if (!this.refreshToken) {
      this.refreshToken = localStorage.getItem('refreshToken');
    }

    if (!this.refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await fetch(`${this.baseUrl}/refresh-token`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        refreshToken: this.refreshToken
      })
    });

    if (!response.ok) {
      throw new Error('Refresh token failed');
    }

    const result = await response.json();
    this.accessToken = result.accessToken;
    this.refreshToken = result.refreshToken;
    localStorage.setItem('accessToken', this.accessToken);
    localStorage.setItem('refreshToken', this.refreshToken);
    return result;
  }

  async logout() {
    if (!this.refreshToken) {
      this.refreshToken = localStorage.getItem('refreshToken');
    }

    if (!this.refreshToken) {
      return;
    }

    await fetch(`${this.baseUrl}/logout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        refreshToken: this.refreshToken
      })
    });

    this.accessToken = null;
    this.refreshToken = null;
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  async graphqlQuery(query: string, variables?: any) {
    if (!this.accessToken) {
      this.accessToken = localStorage.getItem('accessToken');
    }

    const response = await fetch(this.graphqlUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(this.accessToken && { 'Authorization': `Bearer ${this.accessToken}` })
      },
      body: JSON.stringify({ query, variables })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const result = await response.json();
    
    if (result.errors) {
      // If token expired, try refresh
      if (result.errors.some((e: any) => e.message?.includes('token') || e.message?.includes('unauthorized'))) {
        await this.refreshAccessToken();
        // Retry with new token
        return this.graphqlQuery(query, variables);
      }
      throw new Error(result.errors[0].message);
    }

    return result;
  }

  getAccessToken(): string | null {
    return this.accessToken || localStorage.getItem('accessToken');
  }
}
```

### **Sử dụng:**

```typescript
const authService = new AuthService();

// Register
try {
  const result = await authService.register(
    'customer@example.com',
    'SecurePassword123!',
    'Nguyễn Văn A',
    '0901234567',
    'CUSTOMER'
  );
  console.log('Registered:', result);
} catch (error) {
  console.error('Registration failed:', error);
}

// Login
try {
  const result = await authService.login(
    'customer@example.com',
    'SecurePassword123!'
  );
  console.log('Logged in:', result);
} catch (error) {
  console.error('Login failed:', error);
}

// Make authenticated GraphQL query
const query = `
  query {
    getUsers {
      id
      email
      fullName
    }
  }
`;

try {
  const result = await authService.graphqlQuery(query);
  console.log('Users:', result.data.getUsers);
} catch (error) {
  console.error('Query failed:', error);
}
```

---

## 🧪 TESTING VỚI SWAGGER UI

1. **Mở Swagger UI:**
   ```
   http://localhost:5000/swagger
   ```

2. **Test Register:**
   - Tìm endpoint `POST /api/auth/register`
   - Click "Try it out"
   - Nhập thông tin:
     ```json
     {
       "email": "test@example.com",
       "password": "Test123!",
       "fullName": "Test User",
       "phoneNumber": "0901234567",
       "role": "CUSTOMER"
     }
     ```
   - Click "Execute"
   - Copy `accessToken` và `refreshToken` từ response

3. **Test Login:**
   - Tìm endpoint `POST /api/auth/login`
   - Click "Try it out"
   - Nhập thông tin:
     ```json
     {
       "email": "test@example.com",
       "password": "Test123!"
     }
     ```
   - Click "Execute"

4. **Test GraphQL với Authorization:**
   - Mở GraphQL Playground: `http://localhost:5000/graphql`
   - Copy `accessToken` từ login/register response
   - Thêm vào HTTP Headers:
     ```json
     {
       "Authorization": "Bearer {your_access_token_here}"
     }
     ```
   - Chạy query:
     ```graphql
     query {
       getUsers {
         id
         email
         fullName
       }
     }
     ```

---

## 🔒 BẢO MẬT

### **Best Practices:**

1. **Lưu trữ tokens:**
   - ✅ **Access Token:** Lưu trong memory (JavaScript variable) hoặc secure storage
   - ✅ **Refresh Token:** Lưu trong httpOnly cookie (server-side) hoặc secure storage
   - ❌ **KHÔNG** lưu trong localStorage nếu có nguy cơ XSS

2. **Token Rotation:**
   - Mỗi lần refresh, refresh token cũ bị vô hiệu hóa
   - Refresh token mới được tạo và trả về

3. **Token Expiration:**
   - Access Token: 15 phút (có thể config trong appsettings)
   - Refresh Token: 7 ngày (có thể config trong appsettings)

4. **HTTPS:**
   - Luôn sử dụng HTTPS trong production
   - Tokens được truyền qua network, cần mã hóa

---

## 🐛 TROUBLESHOOTING

### **Lỗi: "Email already registered"**
- Email đã tồn tại trong hệ thống
- Sử dụng email khác hoặc login thay vì register

### **Lỗi: "Invalid email or password"**
- Kiểm tra email và password
- Đảm bảo user đã được register

### **Lỗi: "Invalid or expired refresh token"**
- Refresh token đã hết hạn (7 ngày)
- Refresh token đã bị revoke (logout)
- Cần login lại

### **Lỗi: "Invalid refresh token format"**
- Refresh token không đúng format
- Đảm bảo sử dụng raw token từ response, không encrypt

### **Lỗi JWT: "Token validation failed"**
- Access token hết hạn
- Sử dụng refresh token để lấy access token mới
- Hoặc login lại

---

## 📚 TÀI LIỆU THAM KHẢO

- **Swagger UI:** `http://localhost:5000/swagger`
- **GraphQL Playground:** `http://localhost:5000/graphql`
- **JWT.io:** https://jwt.io (để decode và kiểm tra JWT token)

---

## ✅ CHECKLIST TRIỂN KHAI

- [x] Cấu hình `appsettings.json` với JWT settings
- [x] Tạo database migration cho `AuthData` table
- [x] Test Register endpoint
- [x] Test Login endpoint
- [x] Test RefreshToken endpoint
- [x] Test Logout endpoint
- [x] Test authenticated GraphQL queries với access token
- [x] Cấu hình HTTPS trong production
- [x] Thay đổi secret keys trong production

---

## 📝 WORKFLOW

```
1. Client → POST /api/auth/register (REST API)
   ↓
2. Backend → MediatR RegisterCommand
   ↓
3. Handler → IAuthService.RegisterAsync()
   ↓
4. Response → AuthResult (accessToken + refreshToken)
   ↓
5. Client lưu tokens
   ↓
6. Client → GraphQL Query với Authorization: Bearer {accessToken}
   ↓
7. Backend validate JWT token
   ↓
8. Response → GraphQL data
```

---

**Tài liệu này cung cấp hướng dẫn đầy đủ để sử dụng REST API Authentication và GraphQL Queries trong SmartService Platform.**
