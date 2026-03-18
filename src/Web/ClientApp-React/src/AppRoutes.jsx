import { Counter } from "./components/Counter";
import { Customers } from "./components/Customers";
import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";
import { LoginPage } from "./components/api-authorization/LoginPage";
import { ProtectedRoute } from "./components/api-authorization/ProtectedRoute";
import { RegisterPage } from "./components/api-authorization/RegisterPage";

const AppRoutes = [
  {
    index: true,
    element: <Home />,
  },
  {
    path: "/counter",
    element: <Counter />,
  },
  {
    path: "/customers",
    element: <Customers />,
  },
  {
    path: "/fetch-data",
    element: (
      <ProtectedRoute>
        <FetchData />
      </ProtectedRoute>
    ),
  },
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
];

export default AppRoutes;
