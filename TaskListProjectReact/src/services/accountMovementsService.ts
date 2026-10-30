import { authUtils } from '../utils/auth';

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

interface AccountBalance {
  balance: number;
  userId: string;
}

export interface CreateMovementRequest {
  userId: string;
  amount: number;
  type: string;
  date: string;
  description: string | null;
}

const getAuthHeaders = () => {
  const token = authUtils.getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  return headers;
};

export const getAccountMovements = async (
  userId: string,
  page: number = 1,
  pageSize: number = 10
): Promise<AccountMovementsResponse> => {
  try {
    const response = await fetch(
      `/api/AccountMovements/by-user/${userId}?page=${page}&pageSize=${pageSize}`,
      {
        headers: getAuthHeaders(),
      }
    );

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data: AccountMovementsResponse = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching account movements:', error);
    throw error;
  }
};

export const getAccountBalance = async (userId: string): Promise<AccountBalance> => {
  try {
    const response = await fetch(
      `/api/AccountMovements/balance/${userId}`,
      {
        headers: getAuthHeaders(),
      }
    );

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data: AccountBalance = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching account balance:', error);
    throw error;
  }
};

export const createAccountMovement = async (movement: CreateMovementRequest): Promise<AccountMovement> => {
  try {
    const response = await fetch(
      `/api/AccountMovements`,
      {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(movement),
      }
    );

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `HTTP error! status: ${response.status}`);
    }

    const data: AccountMovement = await response.json();
    return data;
  } catch (error) {
    console.error('Error creating account movement:', error);
    throw error;
  }
};
