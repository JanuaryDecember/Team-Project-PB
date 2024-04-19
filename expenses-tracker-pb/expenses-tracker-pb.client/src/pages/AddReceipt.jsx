import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { showSuccessAlert, showFailedAlert, showWarningAlert } from "../components/ToastifyAlert";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const AddReceipt = () => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [previewImage, setPreviewImage] = useState(null);

    const handleFileChange = (event) => {
        const file = event.target.files[0];
        if (file) {
            const fileType = file.type.split('/')[0];
            if (fileType === 'image') {
                setSelectedFile(file);
                const reader = new FileReader();
                reader.onload = () => {
                    setPreviewImage(reader.result);
                };
                reader.readAsDataURL(file);
            } else {
                alert('You have to choose an image.');
                event.target.value = null;
                setSelectedFile(null);
                setPreviewImage(null);
                return;
            }
        }
    };

    const handleUpload = async () => {
        if (!selectedFile) {
            showFailedAlert('You have to choose an image.');
            return;
        }

        const formData = new FormData();
        formData.append('file', selectedFile);

        try {
            const response = await fetch("/api/receipt/upload", {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                if (response.headers.get("content-type").includes("application/json")) {
                    const result = await response.json();
                    alert('Plik został pomyślnie przesłany.');
                    console.log('Ścieżka do pliku:', result.path);
                } else {
                    const text = await response.text();
                    console.error('Odpowiedź nie jest JSON:', text);
                }
                setSelectedFile(null);
                setPreviewImage(null);
            } else {
                console.error('Wystąpił błąd podczas przesyłania pliku.');
                alert('Wystąpił błąd podczas przesyłania pliku.');
            }

        } catch (error) {
            console.error('Błąd:', error);
        }
    };


    return (
        <div className='container'>
            <h2 className="mt-4">Add receipt</h2>
            <div className="mb-3">
                <input className="form-control" type="file" id="formFile" onChange={handleFileChange} accept='image/*' />
                <div style={{ textAlign: 'center' }}>
                    {previewImage && <img src={previewImage} alt="Preview" style={{ maxWidth: '100%', marginTop: '10px', maxHeight: '500px' }} />}
                </div>
                <div className="row my-3"><button className="btn btn-primary" onClick={handleUpload}>Upload</button></div>
            </div>
        </div>
    );
};

export default AddReceipt;

