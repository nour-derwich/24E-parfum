# React Frontend Structure for Perfume Shop

## Project Setup
```bash
npx create-react-app perfume-shop-frontend
cd perfume-shop-frontend
npm install axios react-router-dom @mui/material @emotion/react @emotion/styled
npm install @mui/icons-material @mui/lab
npm install react-hook-form yup @hookform/resolvers
npm install react-toastify
```

## File Structure

```
src/
├── components/
│   ├── common/
│   │   ├── Header.js
│   │   ├── Footer.js
│   │   ├── Sidebar.js
│   │   ├── LoadingSpinner.js
│   │   ├── ErrorMessage.js
│   │   └── ConfirmDialog.js
│   ├── auth/
│   │   ├── LoginForm.js
│   │   ├── RegisterForm.js
│   │   └── ProtectedRoute.js
│   ├── dashboard/
│   │   ├── AdminDashboard.js
│   │   ├── SupplierDashboard.js
│   │   ├── ClientDashboard.js
│   │   └── DashboardCard.js
│   ├── perfumes/
│   │   ├── PerfumeList.js
│   │   ├── PerfumeCard.js
│   │   ├── PerfumeDetails.js
│   │   ├── PerfumeForm.js
│   │   └── PerfumeFilter.js
│   ├── components/
│   │   ├── ComponentList.js
│   │   ├── ComponentCard.js
│   │   ├── ComponentForm.js
│   │   └── ComponentFilter.js
│   ├── orders/
│   │   ├── OrderList.js
│   │   ├── OrderCard.js
│   │   ├── OrderDetails.js
│   │   ├── OrderForm.js
│   │   ├── CustomOrderForm.js
│   │   └── OrderStatus.js
│   └── cart/
│       ├── Cart.js
│       ├── CartItem.js
│       └── CartSummary.js
├── pages/
│   ├── HomePage.js
│   ├── LoginPage.js
│   ├── RegisterPage.js
│   ├── DashboardPage.js
│   ├── PerfumeCatalogPage.js
│   ├── PerfumeDetailsPage.js
│   ├── ComponentsPage.js
│   ├── OrdersPage.js
│   ├── OrderDetailsPage.js
│   ├── CartPage.js
│   ├── ProfilePage.js
│   └── NotFoundPage.js
├── services/
│   ├── api.js
│   ├── authService.js
│   ├── perfumeService.js
│   ├── componentService.js
│   ├── orderService.js
│   └── dashboardService.js
├── hooks/
│   ├── useAuth.js
│   ├── useApi.js
│   ├── useCart.js
│   └── useLocalStorage.js
├── context/
│   ├── AuthContext.js
│   ├── CartContext.js
│   └── ThemeContext.js
├── utils/
│   ├── constants.js
│   ├── helpers.js
│   ├── validation.js
│   └── formatters.js
├── styles/
│   ├── globals.css
│   ├── components.css
│   └── themes.js
├── App.js
├── App.css
└── index.js
```

## Core Files Description

### 1. **App.js** - Main Application Component
```javascript
// Router setup, theme provider, context providers
// Route definitions for all pages
// Protected routes based on user roles
```

### 2. **Services Layer**

#### **api.js** - Base API Configuration
```javascript
// Axios instance with base URL
// Request/response interceptors
// Token management
// Error handling
```

#### **authService.js** - Authentication Services
```javascript
// login(email, password)
// register(userData)
// logout()
// getCurrentUser()
// refreshToken()
```

#### **perfumeService.js** - Perfume API Services
```javascript
// getAllPerfumes()
// getPerfumeById(id)
// createPerfume(perfumeData)
// updatePerfume(id, perfumeData)
// deletePerfume(id)
// getPerfumesBySupplier(supplierId)
```

#### **componentService.js** - Component API Services
```javascript
// getAllComponents()
// getComponentById(id)
// createComponent(componentData)
// updateComponent(id, componentData)
// deleteComponent(id)
// getComponentsBySupplier(supplierId)
```

#### **orderService.js** - Order API Services
```javascript
// getAllOrders()
// getOrderById(id)
// createOrder(orderData)
// updateOrderStatus(id, status)
// getOrdersByClient(clientId)
// createCustomOrder(customOrderData)
```

#### **dashboardService.js** - Dashboard API Services
```javascript
// getDashboardData(role)
// getAdminStats()
// getSupplierStats()
// getClientStats()
```

### 3. **Context Providers**

#### **AuthContext.js** - Authentication State Management
```javascript
// Current user state
// Authentication methods
// Role-based access control
// Token management
```

#### **CartContext.js** - Shopping Cart State Management
```javascript
// Cart items state
// Add/remove items
// Calculate total
// Clear cart
```

### 4. **Custom Hooks**

#### **useAuth.js** - Authentication Hook
```javascript
// Login/logout functionality
// User state management
// Role checking utilities
```

#### **useApi.js** - API Data Fetching Hook
```javascript
// Generic API call hook
// Loading states
// Error handling
// Data caching
```

#### **useCart.js** - Cart Management Hook
```javascript
// Cart operations
// Local storage persistence
// Cart calculations
```

### 5. **Key Components**

#### **ProtectedRoute.js** - Route Protection
```javascript
// Role-based route protection
// Redirect to login if not authenticated
// Check user permissions
```

#### **Header.js** - Navigation Component
```javascript
// Navigation menu
// User profile dropdown
// Cart icon with count
// Search functionality
```

#### **PerfumeCard.js** - Perfume Display Component
```javascript
// Perfume image
// Basic info display
// Add to cart button
// Quick view option
```

#### **OrderCard.js** - Order Display Component
```javascript
// Order summary
// Status display
// Action buttons based on role
```

### 6. **Pages Structure**

#### **HomePage.js** - Landing Page
```javascript
// Featured perfumes
// Categories
// Search functionality
// Hero section
```

#### **DashboardPage.js** - Role-based Dashboard
```javascript
// Dynamic dashboard based on user role
// Statistics cards
// Quick actions
// Recent activity
```

#### **PerfumeCatalogPage.js** - Product Catalog
```javascript
// Perfume grid/list view
// Filtering and sorting
// Pagination
// Search functionality
```

#### **OrdersPage.js** - Order Management
```javascript
// Order list based on user role
// Status filtering
// Order creation (for clients)
// Order management (for admins/suppliers)
```

## User Role-Based Features

### **Admin Features**
- View all users, perfumes, components, orders
- Manage user roles
- System statistics
- Order status management

### **Supplier Features**
- Manage own perfumes and components
- View orders for their products
- Update inventory
- Sales statistics

### **Client Features**
- Browse perfume catalog
- Add items to cart
- Place orders (regular and custom)
- View order history
- Manage profile

## State Management Strategy

### **Global State (Context)**
- User authentication
- Shopping cart
- Theme preferences

### **Local State (useState)**
- Form inputs
- Modal visibility
- Loading states
- Component-specific data

### **Server State (Custom hooks)**
- API data fetching
- Caching strategies
- Error handling
- Loading states

## Routing Structure

```javascript
// Public routes
/ - HomePage
/login - LoginPage
/register - RegisterPage
/catalog - PerfumeCatalogPage
/perfume/:id - PerfumeDetailsPage

// Protected routes
/dashboard - DashboardPage (role-based)
/orders - OrdersPage
/orders/:id - OrderDetailsPage
/cart - CartPage
/profile - ProfilePage

// Admin only
/admin/users - UserManagementPage
/admin/reports - ReportsPage

// Supplier only
/supplier/products - SupplierProductsPage
/supplier/components - SupplierComponentsPage
```

## Key Features to Implement

1. **Authentication Flow**
   - Login/Register forms
   - JWT token management
   - Role-based access control

2. **Product Management**
   - CRUD operations for perfumes/components
   - Image upload functionality
   - Inventory management

3. **Order System**
   - Shopping cart functionality
   - Order placement
   - Custom perfume orders
   - Order tracking

4. **Dashboard Analytics**
   - Role-specific dashboards
   - Statistics and charts
   - Recent activity feeds

5. **Responsive Design**
   - Mobile-first approach
   - Touch-friendly interfaces
   - Responsive grid layouts

## Next Steps

1. Set up the basic project structure
2. Implement authentication flow
3. Create API service layer
4. Build core components
5. Implement routing
6. Add role-based features
7. Style and responsive design
8. Testing and optimization

This structure provides a solid foundation for your perfume shop frontend while maintaining clean architecture and scalability.