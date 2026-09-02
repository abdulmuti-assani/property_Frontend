import React from "react";
import Logo from "./common/Logo";
import { dashboardNavbarStyles as s } from "../assets/dummyStyles";
import { HiMenuAlt2, HiOutlineLogout } from "react-icons/hi";
import { useAuth } from "../context/AuthContext";
const DashboardNavbar = ({ onMenuClick }) => {
  const { logout } = useAuth();
  return (
    <header className={s.header}>
      <button onClick={onMenuClick} className={s.menuButton}>
        <HiMenuAlt2 size={24} />
      </button>

      <div className={s.logoContainer}>
        <Logo fontSize="1.25rem" iconSize={18} />
      </div>

      <button
        type="button"
        onClick={logout}
        title="Logout"
        className="ml-auto flex items-center gap-2 py-2 px-3 rounded-xl text-sm font-semibold text-[#dc2626] bg-red-50 border-none cursor-pointer hover:bg-red-100"
      >
        <HiOutlineLogout size={18} />
        Logout
      </button>
    </header>
  );
};

export default DashboardNavbar;
