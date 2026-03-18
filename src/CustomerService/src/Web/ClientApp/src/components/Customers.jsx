import React, { useState, useEffect, useCallback } from 'react';

export function Customers() {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [exporting, setExporting] = useState(false);
  const [exportResult, setExportResult] = useState(null);
  const [error, setError] = useState(null);

  const pageSize = 20;

  const fetchCustomers = useCallback(async (page) => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetch(`/api/Customers?pageNumber=${page}&pageSize=${pageSize}`);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setCustomers(data.items);
      setTotalPages(data.totalPages);
      setTotalCount(data.totalCount);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCustomers(pageNumber);
  }, [pageNumber, fetchCustomers]);

  const handleExportPdf = async () => {
    setExporting(true);
    setExportResult(null);
    try {
      const res = await fetch('/api/Customers/export-pdf', { method: 'POST' });
      const data = await res.json();
      setExportResult({ success: true, jobId: data.jobId, totalBatches: data.totalBatches });
    } catch (err) {
      setExportResult({ success: false, message: err.message });
    } finally {
      setExporting(false);
    }
  };

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1>Customers <small className="text-muted fs-6">({totalCount.toLocaleString()} total)</small></h1>
        <button
          className="btn btn-danger"
          onClick={handleExportPdf}
          disabled={exporting}
        >
          {exporting ? (
            <><span className="spinner-border spinner-border-sm me-2" role="status" /> Exporting...</>
          ) : (
            '📄 Export PDF'
          )}
        </button>
      </div>

      {exportResult && (
        <div className={`alert alert-${exportResult.success ? 'success' : 'danger'} alert-dismissible`} role="alert">
          {exportResult.success
            ? <>Export queued successfully. Job ID: <strong>{exportResult.jobId}</strong> — {exportResult.totalBatches} batch(es) processing.</>
            : <>Export failed: {exportResult.message}</>
          }
          <button type="button" className="btn-close" onClick={() => setExportResult(null)} />
        </div>
      )}

      {error && <div className="alert alert-danger">Error loading customers: {error}</div>}

      {loading ? (
        <div className="text-center py-4">
          <div className="spinner-border" role="status" />
          <p className="mt-2 text-muted">Loading customers...</p>
        </div>
      ) : (
        <>
          <div className="table-responsive">
            <table className="table table-striped table-hover table-sm">
              <thead className="table-dark">
                <tr>
                  <th>ID</th>
                  <th>Customer ID</th>
                  <th>Gender</th>
                  <th>Location</th>
                  <th>DOB</th>
                  <th>Account Balance</th>
                  <th>Transaction Date</th>
                  <th>Status</th>
                  <th>Brand</th>
                  <th>Price</th>
                  <th>Payment Mode</th>
                </tr>
              </thead>
              <tbody>
                {customers.map(c => (
                  <tr key={c.id}>
                    <td>{c.id}</td>
                    <td>{c.customerId}</td>
                    <td>{c.custGender}</td>
                    <td>{c.custLocation}</td>
                    <td>{c.customerDob}</td>
                    <td>{c.custAccountBalance?.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</td>
                    <td>{c.transactionDate}</td>
                    <td>
                      <span className={`badge bg-${c.status === 'Active' ? 'success' : 'secondary'}`}>
                        {c.status}
                      </span>
                    </td>
                    <td>{c.brand}</td>
                    <td>{c.price?.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}</td>
                    <td>{c.paymentMode}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          <nav>
            <ul className="pagination justify-content-center">
              <li className={`page-item ${pageNumber <= 1 ? 'disabled' : ''}`}>
                <button className="page-link" onClick={() => setPageNumber(p => p - 1)}>Previous</button>
              </li>
              <li className="page-item disabled">
                <span className="page-link">Page {pageNumber} of {totalPages}</span>
              </li>
              <li className={`page-item ${pageNumber >= totalPages ? 'disabled' : ''}`}>
                <button className="page-link" onClick={() => setPageNumber(p => p + 1)}>Next</button>
              </li>
            </ul>
          </nav>
        </>
      )}
    </div>
  );
}
