import React, { useEffect, useRef } from 'react';
import Chart from 'chart.js/auto';

const ObligationsChart = ({ totalRepaid, totalObligation }) => {
    const chartRef = useRef(null);

    useEffect(() => {
        const ctx = chartRef.current.getContext('2d');
        const chart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Total Repaid', 'Total Obligation'],
                datasets: [{
                    data: [totalRepaid, totalObligation - totalRepaid],
                    backgroundColor: [
                        'rgba(75, 192, 192, 0.2)',
                        'rgba(255, 99, 132, 0.2)'],
                    borderColor: [
                        'rgba(75, 192, 192, 1)',
                        'rgba(255, 99, 132, 1)'
                    ],
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
            }
        });

        return () => {
            chart.destroy();
        };
    }, [totalRepaid, totalObligation]);

    return (
        <div className="card mx-auto my-5 h-auto background-chart w-75" style={{ border: 'none' }}>
            <p style={{ fontWeight: 'bold', fontSize: '22px' }}>Report</p>
            <div style={{ height: '300px' }}>
                <canvas ref={chartRef} width="400" height="400"></canvas>
            </div>
        </div>
    );
};

export default ObligationsChart;