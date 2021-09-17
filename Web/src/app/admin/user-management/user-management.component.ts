import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/models/user';
import { AdminService } from 'src/app/services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: Partial<User[]>;
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  //τραβάει όλους τους χρήστες με τους ρόλους τους
  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles()
      .subscribe(users => {
        this.users = users;
      });
  }

  //ανοιγει ενα pop up το οποίο γίνεται populate απο τους ρολους του χρήστη (tick σ όσους ανοικει) που διάλεξε ο admin
  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user),
        
      }
    };

    //εδω ο 1ος subscriber περιμένει αν γίνει submit στο RolesModalComponent, κρατάει τους checked roles
    //και με το this.adminService.updateUserRoles(user.username, rolesToUpdate.roles) στέλει στον server ποιον χρήστη θέλει ο admin Και τι ρόλους θα έχει πλέον
    //στο subscribe περνάει και τοπικά στο this.user.roles τους νέους ρόλους 
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.content.updateSelectedRoles.subscribe(values => {
      const rolesToUpdate = {
        roles: [...values.filter(el => el.checked === true).map(el => el.name)]
      };
      if (rolesToUpdate) {
        this.adminService.updateUserRoles(user.username, rolesToUpdate.roles)
          .subscribe(()=> {
            user.roles = [...rolesToUpdate.roles];
          });
      }
    });
  }

  getRolesArray(user) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      {name: 'Admin', value: 'Admin'},
      {name: 'Moderator', value: 'Moderator'},
      {name: 'Member', value: 'Member'}
    ];

    availableRoles.forEach( role => {
      let isMatch = false;

      for (const userRole of userRoles) {
        if (role.name === userRole) {
          isMatch= true;
          role.checked = true;
          roles.push(role);
          break;
        }
      }
      if (!isMatch) {
        role.checked = false;
        roles.push(role);
      }
    });
    return roles;
  }

}
