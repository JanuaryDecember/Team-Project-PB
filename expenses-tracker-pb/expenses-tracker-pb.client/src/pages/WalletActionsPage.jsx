import React, { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useNavigate } from "react-router-dom";

const ObligationsPage = () => {
  const navigate = useNavigate();
  const { walletId } = useParams();

  const OnClickTransactions = async (walletId) => {
    navigate(`/transaction/${walletId}`);
  };

  const OnClickObligations = async (walletId) => {
    navigate(`/obligation/${walletId}`);
  };

  return (
    <div className="container">
      <h2 className="d-flex justify-content-center mt-5">Wallet {walletId}</h2>

      <div className="d-flex justify-content-center mt-5">
        <button
          className="btn btn-secondary mx-1 mh-50 w-25"
          onClick={() => OnClickTransactions(walletId)}
        >
          Transactions
        </button>
        <button
          className="btn btn-secondary mx-1 mh-50 w-25"
          onClick={() => OnClickObligations(walletId)}
        >
          Obligations
        </button>
      </div>
    </div>
  );
};

export default ObligationsPage;
