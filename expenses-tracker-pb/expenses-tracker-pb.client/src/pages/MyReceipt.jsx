import React, { useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { showSuccessAlert, showWarningAlert } from "../components/ToastifyAlert";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

const MyReceipt = () => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [previewImage, setPreviewImage] = useState(null);
    const [fileName, setFileName] = useState("");
    const [folderLink, setFolderLink] = useState(null);

    useEffect(() => {
        async function fetchFolderLink() {
            try {
                const response = await fetch("/api/receipt/userfolder");
                if (response.ok) {
                    const link = await response.text();
                    setFolderLink(link);
                } else {
                    console.error('An error occurred while fetching folder link.');
                }
            } catch (error) {
                console.error('Error:', error);
            }
        }
        fetchFolderLink();
    }, []);


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
        if (!selectedFile || !fileName) {
            showWarningAlert('You have to choose a file and provide a name.');
            return;
        }

        const formData = new FormData();
        formData.append('file', selectedFile);
        formData.append('fileName', fileName);

        try {
            const response = await fetch("/api/receipt/upload", {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                if (response.headers.get("content-type").includes("application/json")) {
                    const result = await response.json();
                    console.log('File path:', result.path);
                } else {
                    const text = await response.text();
                    console.error('Response is not JSON:', text);
                }
                setSelectedFile(null);
                setPreviewImage(null);
                setFileName("");
                showSuccessAlert('File has been successfully uploaded.');
            } else {
                console.error('An error occurred while uploading the file.');
                alert('An error occurred while uploading the file.');
            }

        } catch (error) {
            console.error('Error:', error);
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
    };

    return (
        <div className='container'>
            <h2 className="mt-4">My receipt</h2>
            {folderLink ? (
                <div>
                    <a href={folderLink} target="_blank" rel="noopener noreferrer">Open My Folder</a>
                </div>
            ) : (
                <div>
                    Link does not exist. You need to add some picture!
                </div>
            )}
            <h2 className="mt-4">Add receipt</h2>
            <form onSubmit={handleSubmit}>
                <div className="row">
                    <div className="col-md-6 mb-3">
                        <label htmlFor="firstName" className="form-label">
                            File name:
                        </label>
                        <input
                            type="text"
                            className="form-control"
                            id="firstName"
                            value={fileName}
                            onChange={(e) => setFileName(e.target.value)}
                            required
                        />
                    </div>

                    <div className="col-md-6 mb-3">
                        <label htmlFor="lastName" className="form-label">
                            Choose an image:
                        </label>
                        <input
                            className="form-control"
                            type="file"
                            id="formFile"
                            onChange={handleFileChange}
                            accept='image/*'
                        />
                    </div>
                    <div style={{ textAlign: 'center' }}>
                        <label htmlFor="lastName" className="form-label">
                            Image preview:
                        </label> <br />
                        {previewImage && <img src={previewImage} alt="Preview" style={{ maxWidth: '300px', marginBottom: '20px', maxHeight: '500px' }} />}
                    </div>
                    <button type="button" className="btn btn-primary" onClick={handleUpload}>Upload</button>
                </div>
            </form>
            <ToastContainer />
        </div>
    );
};

export default MyReceipt;
