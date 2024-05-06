import React, { useState } from "react";
import { useEffect } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { useNavigate } from "react-router-dom";
import { showSuccessAlert, showFailedAlert, showWarningAlert} from '../components/ToastifyAlert';


function Budget() {
  
  const [budgets, setBudgets] = useState(null);
  const [userWalletList, setUserWalletList] = useState(null);
  const [budgetCategoriesList, setBudgetCategoriesList] = useState(null);
  const [newBudgetName, setNewBudgetName] = useState([]);
  const [newBudgetTotalIncome, setNewBudgetTotalIncome] = useState([]);
  const [newBudgetTotalExpenditure, setNewBudgetTotalExpenditure] = useState([]);
  const [newBudgetWalletId, setnewBudgetWalletId] = useState([]);
  const [newBudgetBudgetCategoryId, setnewBudgetBudgetCategoryId] = useState();
  const [alertDanger, setAlertDanger] = useState(null);
  const [alertSuccess, setAlertSuccess] = useState(null);
  const [selectedBudget, setSelectedBudget] = useState(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteConfirmation, setShowDeleteConfirmation] = useState(false);
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
          checkBudget();
        }
      } catch (error) {
        console.error("Error importing wallets:", error.message);
      }
    };
    isUserLogged();
  }, []);

  const checkBudget = async () => {
    try {
        const response = await fetch(`/api/budget/checkBudget`, {
            credentials: 'include',
        });

        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();

        if (data && data.length > 0) {
            console.log("Budgets with negative balance:");
            console.log(data);
            data.forEach(budget => {
                showWarningAlert(`Exceeded Budget!
                Budget: ${budget.name}
                Total Expenditure: ${budget.remainingBalance}
                Category: ${budget.budgetCategoryName}`);
            });
        } else {
            console.log("No budgets have a negative total expenditure.");
        }
    } catch (error) {
        console.error('Error during checking budget:', error);
    }
};

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
        console.log(budgetCategoriesList);
        setBudgetCategoriesList(budgetCategoriesList);
        setnewBudgetBudgetCategoryId(budgetCategoriesList[0].id);
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
        console.log(userWallets);
        setUserWalletList(userWallets);
        setnewBudgetWalletId(userWallets[0].id);
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
          walletId: newBudgetWalletId,
          categoryId: newBudgetBudgetCategoryId,
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

  const handleEditBudget = (budget) => {
    setSelectedBudget(budget);
    setShowEditModal(true);
  };

  const handleSaveBudgetChanges = async(budget) => {
    try {
      console.log("##### DEBUG ######");
      console.log(budget);
      const response = await fetch(`/api/budget/editBudget/${budget.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify(budget),
      });

      if (response.ok) {
        setAlertSuccess("Budget successfully updated!");
        setAlertDanger(null);
        fetchBudgets();
      } else {
        console.error(response);
        setAlertDanger("Failed to update budget!");
      }
    } catch (error) {
      console.error("Error while updating budget:", error);
      setAlertDanger("Failed to update budget!");
    }
    setShowEditModal(false);
  };

  const handleCancelEdit = () => {
    setShowEditModal(false);
  };

  const handleDeleteBudget = (budget) => {
    setSelectedBudget(budget);
    setShowDeleteConfirmation(true);
  };

  const confirmDelete = async(budget) => {
    try {
      const response = await fetch(`/api/budget/deleteBudget/${budget.id}`, {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
      });

      if (response.ok) {
        setAlertSuccess("Budget successfully deleted!");
        setAlertDanger(null);
        fetchBudgets();
      } else {
        console.error(response);
        setAlertDanger("Failed to delete budget!");
      }
    } catch (error) {
      console.error("Error while deleting budget:", error);
      setAlertDanger("Failed to delete budget!");
    }
    setShowDeleteConfirmation(false);
  };

  const cancelDelete = () => {
    setShowDeleteConfirmation(false);
  };

  return (
    <>
      <div className="container">
        <h2 className="m-4">Budget</h2>
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
            <label className="visually-hidden" htmlFor="walletId">Preference</label>
            <select className="form-select" id="walletId" onChange={(e) => setnewBudgetWalletId(e.target.value)} required>
                <option disabled>Wallet</option>
                {userWalletList && userWalletList.map((wallet, index) => (
                  <option key={index} value={wallet.id}>{wallet.name}</option>
                ))}
            </select>
          </div>

          <div className="col-12 m-1">
            <label className="visually-hidden" htmlFor="budgetCategoryId">Preference</label>
            <select className="form-select" id="budgetCategoryId" onChange={(e) => setnewBudgetBudgetCategoryId(e.target.value)} required>
            <option disabled>Budget Category</option>
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
                        RemainingBalance: {budget.remainingBalance}<br />
                        Wallet: {budget.walletName}<br />
                        Category: {budget.budgetCategoryName}
                    </p>
                    <div className="position-absolute top-0 end-0 m-2">
                      <button onClick={() => handleEditBudget(budget)} className="btn btn-warning btn-sm me-2">Edit</button>
                      <button onClick={() => handleDeleteBudget(budget)} className="btn btn-danger btn-sm">Delete</button>
                    </div>
                  </div>
                </div>
              
          ))}
          </div>
        </>
        )}

      {showEditModal && (
        <div className="modal fade show" tabIndex="-1" style={{ display: "block", backgroundColor: "rgba(0, 0, 0, 0.5)" }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Budget</h5>
                <button type="button" className="btn-close" aria-label="Close" onClick={handleCancelEdit}></button>
              </div>
              <div className="modal-body">
              {selectedBudget && (
                <form>
                  <div className="mb-3">
                    <label htmlFor="name" className="form-label">Name</label>
                    <input type="text" className="form-control" id="name" value={selectedBudget.name} onChange={(e) => setSelectedBudget({ ...selectedBudget, name: e.target.value })} />
                  </div>
                  <div className="mb-3">
                    <label htmlFor="totalIncome" className="form-label">Total Income</label>
                    <input type="number" className="form-control" id="totalIncome" value={selectedBudget.totalIncome} onChange={(e) => setSelectedBudget({ ...selectedBudget, totalIncome: e.target.value })} />
                  </div>
                  <div className="mb-3">
                    <label htmlFor="totalExpenditure" className="form-label">Total Expenditure</label>
                    <input type="number" className="form-control" id="totalExpenditure" value={selectedBudget.totalExpenditure} onChange={(e) => setSelectedBudget({ ...selectedBudget, totalExpenditure: e.target.value })} />
                  </div>
                  <div className="mb-3">
                    <label htmlFor="wallet" className="form-label">Wallet</label>
                    <select className="form-select" id="wallet" onChange={(e) => setSelectedBudget({ ...selectedBudget, walletId: e.target.value })} required>
                      <option disabled>Select Wallet</option>
                      {userWalletList && userWalletList.map((wallet, index) => (
                        <option key={index} value={wallet.id}>{wallet.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label htmlFor="budgetCategory" className="form-label">Budget Category</label>
                    <select className="form-select" id="budgetCategory" value={selectedBudget.budgetCategory} onChange={(e) => setSelectedBudget({ ...selectedBudget, categoryId: e.target.value })} required>
                      <option disabled>Select Budget Category</option>
                      {budgetCategoriesList && budgetCategoriesList.map((category, index) => (
                        <option key={index} value={category.id}>{category.name}</option>
                      ))}
                    </select>
                  </div>
                </form>
              )}
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-success" onClick={()=>handleSaveBudgetChanges(selectedBudget)}>Save</button>
                <button type="button" className="btn btn-danger" onClick={handleCancelEdit}>Cancel</button>
              </div>
            </div>
          </div>
        </div>
      )}
      
      {showDeleteConfirmation && (
        <div className="modal fade show" tabIndex="-1" style={{ display: "block", backgroundColor: "rgba(0, 0, 0, 0.5)" }}>
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Confirm Delete</h5>
                <button type="button" className="btn-close" aria-label="Close" onClick={cancelDelete}></button>
              </div>
              <div className="modal-body">
                <p>Are you sure you want to delete the budget "{selectedBudget.name}"?</p>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={cancelDelete}>Cancel</button>
                <button type="button" className="btn btn-danger" onClick={() => confirmDelete(selectedBudget)}>Delete</button>
              </div>
            </div>
          </div>
        </div>
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