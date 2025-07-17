import React, { useState } from 'react';
import './App.css';


function App() {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    eventId: 1
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
      const res = await fetch('/api/RegisterAttendee', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.error || 'Something went wrong.');
      }

      const data = await res.json();
      setResponse(data);
    } catch (err) {
      setError(err.message);
    }
  };

  const styles = {
    container: {
      fontFamily: 'Arial, sans-serif',
      textAlign: 'center',
      padding: '20px',
    },
    form: {
      marginBottom: '20px',
    },
    input: {
      padding: '8px',
      margin: '5px',
      width: '200px',
    },
    button: {
      padding: '10px 20px',
      marginTop: '10px',
    },
    success: {
      color: 'green',
    },
    error: {
      color: 'red',
    },
  };

  return (
    <div style={styles.container}>
      <h1>Event Registration</h1>
      <form onSubmit={handleSubmit} style={styles.form}>
        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          required
          style={styles.input}
        />
        <br />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          required
          style={styles.input}
        />
        <br />
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          required
          style={styles.input}
        />
        <br />
        <button type="submit" style={styles.button}>Register</button>
      </form>

      {response && (
        <div style={styles.success}>
          <h3>Registration Successful!</h3>
          <p><strong>Token:</strong> {response.token}</p>
          <a href={response.qrCodeUrl} target="_blank" rel="noopener noreferrer">
            View QR Code
          </a>
        </div>
      )}

      {error && (
        <div style={styles.error}>
          <p>Error: {error}</p>
        </div>
      )}
    </div>
  );
}

export default App;
