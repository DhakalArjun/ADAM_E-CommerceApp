var dataTable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        'ajax': { url: '/admin/product/getall' },    
        'columns':[
            { data: 'title', 'width': '25%' },
            { data: 'isbn', 'width': '10%' },
            { data: 'listPrice', 'width': '10%' },
            { data: 'author', 'width': '15%' },
            { data: 'category.name', 'width': '15%' },
            {
                data: 'productId',
                'render': function (data) {
                    return`<div class="py-0 text-center">
                        <a href="/admin/product/upsert?id=${data}" class="btn btn-outline-primary btn-sm mx-2 mb-0 py-0" style="width:80px"><i class="bi bi-pencil-square"></i> Edit</a>
                        <a onClick=Delete("/admin/product/delete/${data}") class="btn btn-outline-danger btn-sm mx-2 py-0" style="width:80px"><i class="bi bi-trash"></i> Delete</a>
                    </div>`
                },
                'width': '25%'
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