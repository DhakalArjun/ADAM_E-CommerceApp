var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        'ajax': { url: '/admin/company/getall' },    
        'columns':[
            { data: 'companyName', 'width': '20%' },
            { data: 'streetAddress', 'width': '18%' },
            { data: 'city', 'width': '12%' },
            { data: 'state', 'width': '12%' },
            { data: 'postalCode', 'width': '10%' },
            { data: 'phoneNumber', 'width': '10%' },
            {
                data: 'companyId',
                'render': function (data) {
                    return`<div class="py-0 text-center">
                        <a href="/admin/company/upsert?id=${data}" class="btn btn-outline-primary btn-sm mx-2 mb-0 py-0" style="width:80px"><i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete("/admin/company/delete/${data}") class="btn btn-outline-danger btn-sm mx-2 py-0" style="width:80px"><i class="bi bi-trash"></i> Delete</a>
                    </div>`
                },
                'width': '18%'
            } 
        ]
    });    
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure to delete?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();     //to access dataTable we must have declare it
                    toastr.success(data.message);
                }

            })
        }
    })
}