import React, { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import ObligationForm from "../components/ObligationForm";
import { useNavigate } from "react-router-dom";
import { showSuccessAlert, showFailedAlert, showWarningAlert } from "../components/ToastifyAlert";

const ObligationsPage = () => {
    const { walletId } = useParams();
    const [obligations, setObligations] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [showAddForm, setShowAddForm] = useState(false);
    const [repayAmount, setRepayAmount] = useState("");
    const [selectedObligationId, setSelectedObligationId] = useState(null);
    const [repayObligationId, setRepayObligationId] = useState(null);
    const [repaidAmounts, setRepaidAmounts] = useState({});
    const [selectedCategory, setSelectedCategory] = useState("");
    const [alertMessage, setAlertMessage] = useState("");
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        checkUserLogin();
        fetchObligations();
    }, [walletId, selectedCategory]);

    useEffect(() => {
        if (!isLoggedIn) {
            setAlertMessage("You are not logged in. Redirecting to login page...");
            const redirectTimer = setTimeout(() => {
                navigate("/");
            }, 3000);

            return () => clearTimeout(redirectTimer);
        }
    }, [isLoggedIn, navigate]);

    const checkUserLogin = async () => {
        try {
            const response = await fetch("/api/obligation/user", {
                method: "POST",
                credentials: "include",
            });

            if (response.ok) {
                setIsLoggedIn(true);
            } else {
                setIsLoggedIn(false);
            }
        } catch (error) {
            console.error("Error checking user login:", error);
        }
    };

    const fetchObligations = () => {
        setLoading(true);
        const url = selectedCategory ? `/api/obligation/getObligations/${walletId}?categoryName=${selectedCategory}` : `/api/obligation/getObligations/${walletId}`;
        fetch(url)
            .then((response) => {
                if (!response.ok) {
                    throw new Error("Failed to fetch obligations");
                }
                return response.json();
            })
            .then((data) => {
                setObligations(data);
            })
            .catch((error) => {
                setError(error.message);
            })
            .finally(() => {
                setLoading(false);
            });
    };

    const handleEdit = (obligationId) => {
        setSelectedObligationId(obligationId);
    };

    const handleSave = async (obligationId, updatedObligation) => {
        try {
            const response = await fetch(`/api/obligation/updateObligation`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(updatedObligation),
            });
            if (!response.ok) {
                throw new Error("Failed to save obligation");
            }
            const updatedObligations = obligations.map(obligation => {
                if (obligation.id === obligationId) {
                    return { ...obligation, editing: false };
                } else {
                    return obligation;
                }
            });
            showSuccessAlert('Obligation added successfully!')
            setObligations(updatedObligations);
            setSelectedObligationId(null);
        } catch (error) {
            showFailedAlert('Error saving obligation!')
            console.error("Error saving obligation:", error);
        }
    };

    const handleDelete = async (obligationId) => {
        try {
            const response = await fetch(`/api/obligation/deleteObligation/${obligationId}`, {
                method: "DELETE",
            });
            if (!response.ok) {
                throw new Error("Failed to delete obligation");
            }
            showSuccessAlert('Obligation deleted successfully!')
            setObligations(obligations.filter(obligation => obligation.id !== obligationId));
        } catch (error) {
            showFailedAlert('Error deleting obligation!')
            console.error("Error deleting obligation:", error);
        }
    };

    const handleRepay = async (obligationId) => {
        setRepayObligationId(obligationId);
    };

    const handleConfirm = async (obligationId) => {
        setRepayObligationId(null);

        var obligation = repaidAmounts[obligationId] || 0;
        if (parseFloat(repayAmount) + parseFloat(obligation) < parseFloat(obligations.find(obligation => obligation.id === obligationId).amount)) {
            setRepaidAmounts(prevState => ({
                ...prevState,
                [obligationId]: (prevState[obligationId] || 0) + parseFloat(repayAmount)
            }));

            try {
                const response = await fetch(`/api/obligation/repayObligations/${obligationId}?amount=${repayAmount}`, {
                    method: "POST",
                });
                if (!response.ok) {
                    throw new Error("Failed to repay obligation");
                }
            } catch (error) {
                showFailedAlert('Error repaying obligation!')
                console.error("Error repaying obligation:", error);
            }
        }

        setRepayAmount("");
    }

    const handleToggleAddForm = () => {
        setShowAddForm(!showAddForm);
    };

    return (
        <div className="container mt-5">
            <>
                <h2>Obligations</h2>
                <button className="btn btn-primary" style={{ marginTop: "10px" }} onClick={handleToggleAddForm}>Add Obligation</button>
                {showAddForm && <ObligationForm walletId={walletId} refreshObligationsList={fetchObligations} />}
                <h3 style={{ marginTop: "20px" }}>Filter:</h3>
                <select className="form-select" value={selectedCategory} onChange={(e) => setSelectedCategory(e.target.value)}>
                    <option value="">All Categories</option>
                    <option value="Credit">Credit</option>
                    <option value="Leasing">Leasing</option>
                    <option value="Fees">Fees</option>
                </select>
                <div className="row my-3">
                    {obligations.map((obligation, index) => (
                        <div
                            key={index}
                            className="col my-3"
                            style={{ minWidth: "30%" }}
                        >
                            <div className="card h-100 w-100 text-center">
                                {selectedObligationId === obligation.id ? (
                                    <React.Fragment>
                                        <div className="row mb-3">
                                            <div className="col-md-6">
                                                <label className="form-label">Name:</label>
                                                <input
                                                    type="text"
                                                    className="form-control"
                                                    value={obligation.name}
                                                    onChange={(e) => {
                                                        const updatedObligations = [...obligations];
                                                        updatedObligations[index].name = e.target.value;
                                                        setObligations(updatedObligations);
                                                    }}
                                                />
                                            </div>
                                            <div className="col-md-6">
                                                <label className="form-label">Amount:</label>
                                                <input
                                                    type="number"
                                                    className="form-control"
                                                    value={obligation.amount}
                                                    onChange={(e) => {
                                                        const updatedObligations = [...obligations];
                                                        updatedObligations[index].amount = e.target.value;
                                                        setObligations(updatedObligations);
                                                    }}
                                                />
                                            </div>
                                        </div>
                                        <div className="row mb-3">
                                            <div className="col-md-6">
                                                <label className="form-label">Start Date:</label>
                                                <input
                                                    type="date"
                                                    className="form-control"
                                                    value={obligation.startDate}
                                                    onChange={(e) => {
                                                        const updatedObligations = [...obligations];
                                                        updatedObligations[index].startDate = e.target.value;
                                                        setObligations(updatedObligations);
                                                    }}
                                                />
                                            </div>
                                            <div className="col-md-6">
                                                <label className="form-label">Due Date:</label>
                                                <input
                                                    type="date"
                                                    className="form-control"
                                                    value={obligation.dueDate}
                                                    onChange={(e) => {
                                                        const updatedObligations = [...obligations];
                                                        updatedObligations[index].dueDate = e.target.value;
                                                        setObligations(updatedObligations);
                                                    }}
                                                />
                                            </div>
                                        </div>
                                        <button className="btn btn-dark" onClick={() => handleSave(obligation.id, obligation)}>Save</button>
                                    </React.Fragment>
                                ) : (
                                    <React.Fragment>
                                        <h2 className="w-75">{obligation.name}</h2>
                                        <h5>{obligation.amount}</h5>
                                        <h5>Category: {obligation.category.name}</h5>
                                        <h5>Start Date: {new Date(obligation.startDate).toLocaleDateString()}</h5>
                                        <h5>Due Date: {new Date(obligation.dueDate).toLocaleDateString()}</h5>
                                        <h5>Progress: {`${(repaidAmounts[obligation.id] || 0) + obligation.paidAmount}/${obligation.amount}`}</h5>
                                        {!obligation.editing && repayObligationId !== obligation.id && (
                                            <button className="btn btn-primary" onClick={() => handleRepay(obligation.id)}>Repay</button>
                                        )}
                                        {repayObligationId === obligation.id && !obligation.editing && (
                                            <div>
                                                <input
                                                    type="number"
                                                    value={repayAmount}
                                                    onChange={(e) => setRepayAmount(e.target.value)}
                                                    placeholder="Enter amount"
                                                    className="form-control"
                                                />
                                                <div style={{ marginTop: "10px" }}>
                                                    <button className="btn btn-primary" onClick={() => handleConfirm(obligation.id)}>Confirm</button>
                                                </div>
                                            </div>
                                        )}
                                        <button className="btn btn-dark" style={{ marginTop: "5px" }} onClick={() => handleEdit(obligation.id)}>Edit</button>
                                        <button className="btn btn-danger" style={{ marginTop: "5px" }} onClick={() => handleDelete(obligation.id)}>Delete</button>
                                    </React.Fragment>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
                {!isLoggedIn && alertMessage !== "" && (
                    <div className="alert alert-danger" role="alert">
                        {alertMessage}
                    </div>
                )}
            </>
        </div>
    );
};

export default ObligationsPage;
