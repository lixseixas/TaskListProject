import { useState, useEffect, useCallback } from 'react'
import { getAccountMovements, getAccountBalance, createAccountMovement } from '../services/accountMovementsService'
import type { CreateMovementRequest } from '../services/accountMovementsService'
import './Home.css'

interface AccountMovement {
  id: string;
  userId: string;
  amount: number;
  type: string;
  date: string;
  description: string | null;
}

interface AccountMovementsResponse {
  movements: AccountMovement[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

function Home() {
  const [movements, setMovements] = useState<AccountMovementsResponse | null>(null);
  const [balance, setBalance] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const [balanceLoading, setBalanceLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  
  // Form state for new movement
  const [newAmount, setNewAmount] = useState('');
  const [newType, setNewType] = useState('Credit');
  const [newDescription, setNewDescription] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const userId = '785c2e15-8464-4cea-b30e-99faab345eb0'; // Test user ID

  const loadMovements = useCallback(async (page: number) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAccountMovements(userId, page, 10);
      setMovements(data);
    } catch (err) {
      setError('Failed to load account movements');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [userId]);

  const loadBalance = useCallback(async () => {
    try {
      setBalanceLoading(true);
      const data = await getAccountBalance(userId);
      setBalance(data.balance);
    } catch (err) {
      console.error('Error loading balance:', err);
      setBalance(0);
    } finally {
      setBalanceLoading(false);
    }
  }, [userId]);

  useEffect(() => {
    loadMovements(currentPage);
  }, [currentPage, loadMovements]);

  useEffect(() => {
    loadBalance();
  }, []);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const formatAmount = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const formatBalance = (amount: number | null) => {
    if (amount === null) return 'Loading...';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const handleSubmitMovement = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newAmount || isNaN(parseFloat(newAmount))) {
      alert('Please enter a valid amount');
      return;
    }

    try {
      setSubmitting(true);
      
      const movementRequest: CreateMovementRequest = {
        userId: userId,
        amount: parseFloat(newAmount),
        type: newType,
        date: new Date().toISOString(),
        description: newDescription || null
      };

      await createAccountMovement(movementRequest);
      
      // Reset form
      setNewAmount('');
      setNewDescription('');
      setNewType('Credit');
      
      // Refresh data
      await loadMovements(currentPage);
      await loadBalance();
      
    } catch (err: any) {
      console.error('Error creating movement:', err);
      
      // Try to get the error message from the response
      let errorMessage = 'Failed to create movement.';
      if (err.message) {
        errorMessage = err.message;
      } else if (err.response && err.response.data) {
        errorMessage = err.response.data;
      }
      
      alert(errorMessage);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="app-container">
      <h1>Account Movements</h1>
      
      <div className="balance-container">
        <div className="balance-label">Current Balance</div>
        {balanceLoading ? (
          <div className="balance-loading">Loading balance...</div>
        ) : (
          <p className="balance-amount">{formatBalance(balance)}</p>
        )}
      </div>

      <div className="add-movement-form">
        <h3>Add New Movement</h3>
        <form onSubmit={handleSubmitMovement}>
          <div className="form-row">
            <div className="form-group">
              <label htmlFor="amount">Amount</label>
              <input
                type="number"
                id="amount"
                value={newAmount}
                onChange={(e) => setNewAmount(e.target.value)}
                placeholder="Enter amount"
                step="0.01"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="type">Type</label>
              <select
                id="type"
                value={newType}
                onChange={(e) => setNewType(e.target.value)}
              >
                <option value="Credit">Credit</option>
                <option value="Debit">Debit</option>
              </select>
            </div>
          </div>
          <div className="form-group">
            <label htmlFor="description">Description</label>
            <input
              type="text"
              id="description"
              value={newDescription}
              onChange={(e) => setNewDescription(e.target.value)}
              placeholder="Enter description (optional)"
            />
          </div>
          <button 
            type="submit" 
            className="submit-button"
            disabled={submitting}
          >
            {submitting ? 'Adding...' : 'Add Movement'}
          </button>
        </form>
      </div>
      
      {loading && <div className="loading">Loading...</div>}
      
      {error && <div className="error">{error}</div>}
      
      {movements && movements.movements.length > 0 && (
        <>
          <div className="movements-grid">
            <table className="movements-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Amount</th>
                  <th>Description</th>
                </tr>
              </thead>
              <tbody>
                {movements.movements.map((movement) => (
                  <tr key={movement.id}>
                    <td>{formatDate(movement.date)}</td>
                    <td>{movement.type}</td>
                    <td className={movement.amount >= 0 ? 'positive' : 'negative'}>
                      {formatAmount(movement.amount)}
                    </td>
                    <td>{movement.description || '-'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="pagination">
            <button
              onClick={() => setCurrentPage((prev) => Math.max(1, prev - 1))}
              disabled={currentPage === 1}
              className="pagination-button"
            >
              Previous
            </button>
            <span className="page-info">
              Page {movements.page} of {movements.totalPages}
            </span>
            <button
              onClick={() => setCurrentPage((prev) => Math.min(movements.totalPages, prev + 1))}
              disabled={currentPage === movements.totalPages}
              className="pagination-button"
            >
              Next
            </button>
          </div>

          <div className="summary">
            <p>Total Movements: {movements.totalCount}</p>
          </div>
        </>
      )}

      {movements && movements.movements.length === 0 && (
        <div className="no-data">No account movements found</div>
      )}
    </div>
  );
}

export default Home