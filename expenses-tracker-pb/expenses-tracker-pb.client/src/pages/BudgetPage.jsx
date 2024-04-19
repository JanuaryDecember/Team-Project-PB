import React, { useState } from "react";
import { useEffect } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";


function Budget() {
  
  const [budgets, setBudgets] = useState(null);
  const [userWalletList, setUserWalletList] = useState(null);
  const [budgetCategoriesList, setBudgetCategoriesList] = useState(null);
  const [newBudgetName, setNewBudgetName] = useState([]);
  const [newBudgetTotalIncome, setNewBudgetTotalIncome] = useState([]);
  const [newBudgetTotalExpenditure, setNewBudgetTotalExpenditure] = useState([]);
  const [newBudgetWallet, setNewBudgetWallet] = useState([]);
  const [newBudgetBudgetCategory, setNewBudgetBudgetCategory] = useState([]);
  const [alertDanger, setAlertDanger] = useState(null);
  const [alertSuccess, setAlertSuccess] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const isUserLogged = async () => {
      try {
        const response = await fetch("/api/import/user", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          credentials: "include",
        });

        if(response.status === 401) {
          setAlertDanger("The user is not logged in!");
          console.error(
            "The user is not logged in!",
            response.status,
            response.statusText
            );
            setTimeout(() => {
                navigate("/");
            }, 3000);
        }
        if (response.ok) {
          setAlertDanger(null);
          setAlertSuccess(null);
          fetchBudgets();
          getAllUserWallets();
          getAllBudgetCategories();
        }
      } catch (error) {
        console.error("Error importing wallets:", error.message);
      }
    };
    isUserLogged();
  }, []);

  const getAllBudgetCategories = async () => {
    try {
      const response = await fetch("/api/budget/budgetCategories", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      });

      if (response.ok) {
        const budgetCategoriesList = await response.json();
        console.log("Successfully imported budget categories list.");
        setBudgetCategoriesList(budgetCategoriesList);
      } else {
        console.error(response);
        setAlertDanger("Budget categories are not defined!");
      }
    } catch (error) {
      console.error("Error during fetching budget categories list!", error);
      setAlertDanger("Error fetching budget categories list!");
    }
  }

  const getAllUserWallets = async () => {
    try {
      const response = await fetch("/api/budget/userWallets", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      });

      if (response.ok) {
        const userWallets = await response.json();
        console.log("Successfully imported user wallets list.");
        setUserWalletList(userWallets);
      } else {
        console.error(response);
        setAlertDanger("User dont have any wallets! Create at least one!");
      }
    } catch (error) {
      console.error("Error during fetching user wallets!", error);
      setAlertDanger("Error fetching user wallets!");
    }
  }

  const fetchBudgets = async () => {
    try {
      const response = await fetch("/api/budget/showBudgets", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      });

      if (response.ok) {
        const budgetsData = await response.json();
        setBudgets(budgetsData);
        console.log("Budgets are loaded.");
        console.log(budgetsData);
        if(budgetsData == null)
          setAlertDanger("No defined budgets!");
      } else {
        console.error(response);
        setAlertDanger("Error fetching budgets!");
      }
    } catch (error) {
      console.error("Error during fetching budgets!", error);
      setAlertDanger("Error fetching budgets!");
    }
  };

  const submitNewBudget = async (newBudget) => {
    try{

      const budgetDetails = {
          name: newBudgetName,
          totalIncome: newBudgetTotalIncome,
          totalExpenditure: newBudgetTotalExpenditure,
          budgetCategoryId: newBudgetBudgetCategory,
          walletId: newBudgetWallet,
      }
      console.log(budgetDetails);
      console.log(JSON.stringify(budgetDetails));

      const response = await fetch("/api/budget/createBudget", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(budgetDetails),
      });

      if(response.ok){
        setAlertSuccess("Successfully created new budget!");
        setAlertDanger(null);
      }

    }catch(error){
      console.error("Adding new budget failed.", error);
      setAlertSuccess(null);
      setAlertDanger("Failed to create new Budget!");
    }
  }

  return (
    <>
      <div className="container">
        <h2 className="m-4">Budget</h2>
        {/*formularz do tworzenia nowego budzetu*/}
        <form onSubmit={submitNewBudget} className="row row-cols-lg-auto align-items-center border rounded p-2">
          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="name">Name</label>
            <div className="input-group">
              <div className="input-group-text">@</div>
              <input type="text" className="form-control" id="name" placeholder="Budget name" onChange={(e) => setNewBudgetName(e.target.value)} required/>
            </div>
          </div>

          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="totalIncome">TotalIncome</label>
            <div className="input-group">
              <input type="number" className="form-control" id="totalIncome" placeholder="Total income" onChange={(e) => setNewBudgetTotalIncome(e.target.value)}required/>
            </div>
          </div>

          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="totalExpenditure">TotalExpenditure</label>
            <div className="input-group">
              <input type="number" className="form-control" id="totalExpenditure" placeholder="Total Expenditure" onChange={(e) => setNewBudgetTotalExpenditure(e.target.value)} required/>
            </div>
          </div>

          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="wallets">Preference</label>
            <select className="form-select" id="wallets" onChange={(e) => setNewBudgetWallet(e.target.value)} required>
                <option >Wallet</option>
                {userWalletList && userWalletList.map((wallet, index) => (
                  <option key={index} value={wallet.id}>{wallet.name}</option>
                ))}
            </select>
          </div>

          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="budgetCategories">Preference</label>
            <select className="form-select" id="budgetCategories" onChange={(e) => setNewBudgetBudgetCategory(e.target.value)} required>
            <option >Budget Category</option>
              {budgetCategoriesList && budgetCategoriesList.map((category, index) => (
                <option key={index} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>

          <div className="col-12 m-1">
            <button type="submit" className="btn btn-primary" required>Add new budget</button>
          </div>
        </form>
    
        {/*showing all budgets*/}
        {budgets && (
          <>
          <div className="row my-3">
            {budgets.map((budget, index) => (
                <div key={index} className="col my-2 card h-100 w-100 text-center m-2" style={{ minWidth: "30%" }}>
                  <div className="card-body">
                    <h5 className="card-title">
                      {budget.name}
                    </h5>
                    <p className="card-text">
                        Total Income: {budget.totalIncome}<br/>
                        Total Expenditure: {budget.totalExpenditure}<br/>
                        RemainingBalance: {budget.remainingBalance}
                    </p>
                  </div>
                </div>
              
          ))}
          </div>
        </>
        )}
      
        {alertDanger != null ? (
        <div className="alert alert-danger" style={{ marginTop: "10px" }} role="alert">
            {alertDanger}
          </div>
        ) : undefined}
        {alertSuccess != null ? (
          <div className="alert alert-success" role="alert">
            {alertSuccess}
          </div>
        ) : undefined}
      </div>
    </>
  );
}
export default Budget;