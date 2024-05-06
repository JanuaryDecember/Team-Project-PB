import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { showSuccessAlert, showFailedAlert, showWarningAlert} from './ToastifyAlert';

const TransactionForm = ({ onSubmit, onCancel, walletId }) => {
    const navigate = useNavigate();

    const [newTransaction, setNewTransaction] = useState({
        title: '',
        description: '',
        date: new Date().toISOString().split('T')[0],
        amount: '',
        walletId: walletId,
        categoryId: '',
    });

    const [formError, setFormError] = useState('');
    const [formVisible, setFormVisible] = useState(false);
    const [transactionType, setTransactionType] = useState('');
    const [confirmationVisible, setConfirmationVisible] = useState(false);
    const [categories, setCategories] = useState([]);
    const [useCategoryInput, setUseCategoryInput] = useState(false);
    const [customCategory, setCustomCategory] = useState('');
    const [information, setInformation] = useState('');
    const [selectedFile, setSelectedFile] = useState(null);

    const handleFileChange = (event) => {
        setSelectedFile(event.target.files[0]);
    };

    const handleButtonClick = async () => {
        if (!selectedFile) {
            alert('Please select a file.');
            return;
        }

        const formData = new FormData();
        formData.append('imageFile', selectedFile);

        try {
            document.getElementById("fill-button").setAttribute("disabled", true)
            const response = await fetch('/api/transaction/getTotalPrice', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const result = await response.json();
                const priceMatch = result.content.match(/(\d+(\.\d+)?)/);
                const price = priceMatch ? parseFloat(priceMatch[0]) : null;
                setNewTransaction(prevTransaction => ({
                    ...prevTransaction,
                    amount: price,
                }));
                alert("Price decoded is: " + price);
                document.getElementById("fill-button").removeAttribute("disabled")
            } else {
                document.getElementById("fill-button").removeAttribute("disabled")
                console.error('Failed to fetch');
            }
        } catch (error) {
            document.getElementById("fill-button").removeAttribute("disabled")
            console.error('Error:', error);
        }
    };



    const handleCustomCategoryChange = (event) => {
        setCustomCategory(event.target.value);
    };

    const handleInputChange = (e) => {
        const { name, value } = e.target;

        if (name !== 'category') {
            setNewTransaction(prevTransaction => ({
                ...prevTransaction,
                [name]: value,
            }));
        }
    };

    const handleCategoryChange = (e) => {
        const { value } = e.target;
        setNewTransaction(prevTransaction => ({
            ...prevTransaction,
            categoryId: value,
        }));
    };

    const resetForm = () => {
        setNewTransaction({
            title: '',
            description: '',
            date: new Date().toISOString().split('T')[0],
            amount: '',
            walletId: walletId,
            categoryId: ''
        });
        setCustomCategory('');
        setInformation('');
        setFormError('');
    };

    const fetchCategories = async () => {
        try {
            const response = await fetch(`/api/transaction/allCategories`, {
                credentials: 'include',
            });
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const data = await response.json();
            setCategories(data);
        } catch (error) {
            console.error('Error during fetching categories:', error);
        }
    };

    const handleTypeSelection = (type) => {
        setTransactionType(type);
        setFormVisible(true);
        fetchCategories();
    };

    const findCategoryId = async (cat) => {
        let cats;
        try {
            const response = await fetch(`/api/transaction/allCategories`, {
                credentials: 'include',
            });
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            const data = await response.json();
            cats = data;
        } catch (error) {
            console.error('Error during fetching categories:', error);
        }
        const foundCategory = cats.find(
            (category) => category.Name === cat
        );
        return foundCategory ? foundCategory.Id : 0;
    };

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
    

    const handleSubmit = async (e) => {
        e.preventDefault();
        let newCat = { name: customCategory, type: '' };
        let newTrans = {
            title: newTransaction.title,
            description: newTransaction.description,
            date: newTransaction.date,
            amount: newTransaction.amount,
            walletId: newTransaction.walletId,
            categoryId: newTransaction.categoryId
        }

        if (useCategoryInput) {
            if (transactionType === 'income') {
                newCat.type = "Income";
            } else {
                newCat.type = "Expenditure";
            }
            const foundCategory = categories.find(
                (category) => category.Name === newCat.name
            );

            if (foundCategory === null || foundCategory === undefined) {
                try {
                    const response = await fetch('/api/transaction/addCategory', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify(newCat),
                        credentials: 'include',
                    });

                    if (!response.ok) {
                        setInformation('Error during adding category.');
                        throw new Error('Failed to add category');
                    }

                    setInformation('Category added successfully.');
                } catch (error) {
                    console.error('Error during adding category:', error);
                    setInformation('Error during adding category.');
                }
            }
            else {
                setInformation('Category with that name already exists!');
            }
            fetchCategories();

            newTrans.categoryId = await findCategoryId(newCat.name);
        }


        try {
            let url = '';
            if (transactionType === 'income') {
                url = '/api/transaction/addIncome';
            } else {
                url = '/api/transaction/addExpenditure';
            }

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(newTrans),
                credentials: 'include',
            });

            if (response.ok) {
                resetForm();
                setFormVisible(false);
                setConfirmationVisible(true);
                setTimeout(() => {
                    setConfirmationVisible(false);
                }, 1500);
                showSuccessAlert('Transaction added successfully!');
                checkBudget();
                document.getElementById('helper').click();
            } else {
                console.error(response);
                showFailedAlert('Invalid form data');
                setFormError('Invalid form data');
            }
        } catch (error) {
            console.error('Error adding transaction:', error);
            showFailedAlert('Error adding transaction');
            setFormError('Error adding transaction');
        }
    };

    return (
        <div>
            <div className="mb-3">
                <button
                    className={`btn ${transactionType === 'income' ? 'btn-grey' : 'btn-secondary'} mx-1`}
                    onClick={() => handleTypeSelection('income')}
                >
                    Income
                </button>
                <button
                    className={`btn ${transactionType === 'expenditure' ? 'btn-grey' : 'btn-secondary'} mx-1`}
                    onClick={() => handleTypeSelection('expenditure')}
                >
                    Expenditure
                </button>
            </div>
            {formVisible && (
                <div className="card w-50 h-auto m-auto mb-5 p-3 pt-3">
                    <form onSubmit={handleSubmit} className="row w-100 g-3">
                        <h5>Add New Transaction</h5>

                        <input
                            type="text"
                            className="form-control"
                            name="title"
                            value={newTransaction.title}
                            onChange={handleInputChange}
                            placeholder="Enter title"
                            required
                        />
                        <input
                            type="text"
                            className="form-control"
                            name="description"
                            value={newTransaction.description}
                            onChange={handleInputChange}
                            placeholder="Enter description"
                        />
                        <input
                            type="number"
                            className="form-control"
                            name="amount"
                            value={newTransaction.amount}
                            onChange={handleInputChange}
                            placeholder="Enter amount"
                            required
                        />
                        <input
                            type="date"
                            className="form-control"
                            name="date"
                            value={newTransaction.date}
                            onChange={handleInputChange}
                            required
                        />

                        <div style={{ display: 'flex', alignItems: 'center' }}>
                            <input
                                type="checkbox"
                                id="useCategoryInput"
                                checked={useCategoryInput}
                                onChange={() => setUseCategoryInput(!useCategoryInput)}
                            />
                            <label htmlFor="useCategoryInput" style={{ marginLeft: '8px' }}>
                                Create new category
                            </label>
                        </div>
                        {useCategoryInput ? (
                            <input
                                type="text"
                                className="form-control"
                                name="name"
                                value={customCategory}
                                onChange={handleCustomCategoryChange}
                                placeholder="Enter new category name"
                                required={useCategoryInput}
                            />
                        ) : (
                            <select
                                className="form-select"
                                name="category"
                                value={newTransaction.categoryId}
                                onChange={handleCategoryChange}
                                required={!useCategoryInput}
                            >
                            <option value="">Select category</option>
                            {categories.map((category) => (
                                (transactionType === 'income' && category.Type === 'Income') ||
                                    (transactionType === 'expenditure' && category.Type === 'Expenditure')
                                    ? (
                                        <option key={category.Id} value={category.Id}>
                                            {category.Name}
                                        </option>
                                    ) : null
                            ))}
                            </select>
                        )}
                        {transactionType === 'expenditure' ?
                            <div className="d-flex flex-column w-50">
                                <button id="fill-button" className="btn btn-primary" onClick={handleButtonClick}>
                                    <p>Fill amount with receipt photo</p>
                                </button>
                                <input className="mt-2" type="file" accept="image/*" onChange={handleFileChange}  />
                            </div>
                            : ""}
                        {information && <div className="error">{information}</div>}
                        {formError && <div className="col-md-12 error">{formError}</div>}
                        <button type="submit" className="btn btn-primary col-12">
                            Add
                        </button>
                    </form>
                </div>
            )}
            
        </div>
    );
};

export default TransactionForm;