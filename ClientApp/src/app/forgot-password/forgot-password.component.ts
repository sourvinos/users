import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';
import { AccountService } from '../services/account.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-forgot-password',
    templateUrl: './forgot-password.component.html'
})

export class ForgotPasswordComponent implements OnInit {

    form: FormGroup;
    errorList: string[] = [];

    constructor(private formBuilder: FormBuilder, private accountService: AccountService, private router: Router) { }

    ngOnInit() {
        this.initForm()
    }

    initForm() {
        this.form = this.formBuilder.group({
            email: ['', [Validators.required, Validators.email]],
        })
    }

    onSubmit() {
        const form = this.form.value;
        this.accountService.forgotPassword(form.email).subscribe(() => {
            alert(`An email was sent to ${form.email} for password reset`)
            this.router.navigateByUrl('/login');
        }, error => {
            this.errorList = []
            error.error.response.forEach((element: string) => {
                this.errorList.push(element + '\n');
                console.log(element)
            });
            alert(this.errorList)
        });
    }

    get Email() {
        return this.form.get('email')
    }

}
