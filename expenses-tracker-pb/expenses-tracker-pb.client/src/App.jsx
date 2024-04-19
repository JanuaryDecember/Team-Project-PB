import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import RegisterForm from './pages/RegisterFormPage';
import Navbar from './components/Navbar';

import WalletPage from './pages/WalletPage';
import WalletActionsPage from './pages/WalletActionsPage';
import TransactionList from './pages/TransactionsPage';
import ObligationPage from './pages/ObligationsPage';

import Export from './pages/ExportPage';
import Import from './pages/ImportPage';
import FinancialReport from './pages/FinancialReportPage';
import ManageCategories from './pages/ManageCategoriesPage';
import TwoFactorAuthentication from './account/settings/TwoFactorAuthentication';
import ProfilePage from './pages/ProfilePage'
import PasswordRecoveryPage from './pages/PasswordRecoveryPage'
import LandingPage from './pages/LandingPage';
import DashboardPage from './pages/DashbordPage';
import AddReceipt from './pages/AddReceipt';
import BudgetPage from './pages/BudgetPage';

const App = () => {
    return (
        <Router>
            <Navbar />
            <Routes>
                <Route path="/" element={<LandingPage />} />
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/register" element={<RegisterForm />} />

                <Route path="/wallet" element={<WalletPage />} />
                <Route path="/wallet/actions/:walletId" element={<WalletActionsPage/>} />
                <Route path="/transaction/:walletId" element={<TransactionList />} />
                <Route path="/obligation/:walletId" element={<ObligationPage />} />

                <Route path="/export" element={<Export />} />
                <Route path="/import" element={<Import />} />
                <Route path="/report" element={<FinancialReport />} />
                <Route path="/account/settings/twoFactorAuthentication" element={<TwoFactorAuthentication />} />
                <Route path="/categories" element={<ManageCategories />} />
                <Route path="/profile" element={<ProfilePage />}></Route>
                <Route path="/passwordRecovery" element={<PasswordRecoveryPage />}></Route>
                <Route path="/addReceipt" element={<AddReceipt />}></Route>
                <Route path="/budget" element={<BudgetPage />}></Route>
            </Routes>
        </Router>
    );
};

export default App;
