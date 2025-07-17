import React, { useState } from 'react';
import './App.css';

function App() {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    eventId: 1, // Default value; adjust as needed
  });

  const [response, setResponse] = useState(null);
  const [error, setError] = useState(null);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prevData) => ({
      ...prevData,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setResponse(null);

    try {
      const res = await fetch('/api/EventFunctions/RegisterAttendee', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      const text = await res.text();

      if (!res.ok) {
        // Try to parse error details
        try {
          const errorData = JSON.parse(text);
          throw new Error(errorData.error || `Error ${res.status}`);
        } catch {
          throw new Error(`Unexpected response: ${text}`);
        }
      }

      try {
        const data = JSON.parse(text);
        setResponse(data);
      } catch (err) {
        throw new Error("Invalid JSON response from server.");
      }
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="App">
      <h1>Event Registration</h1>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          required
        />
        <br />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          required
        />
        <br />
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          required
        />
        <br />
        <button type="submit">Register</button>
      </form>

      {response && (
        <div className="success">
          <h3>Registration Successful!</h3>
          <p><strong>Token:</strong> {response.token}</p>
          <a href={response.qrCodeUrl} target="_blank" rel="noopener noreferrer">
            View QR Code
          </a>
        </div>
      )}

      {error && (
        <div className="error">
          <p style={{ color: 'red' }}>Error: {error}</p>
        </div>
      )}
    </div>
  );
}

export default App;
