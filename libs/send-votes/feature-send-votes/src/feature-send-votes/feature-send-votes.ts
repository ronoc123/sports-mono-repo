import {
  Component,
  computed,
  inject,
  signal,
  TemplateRef,
  ViewChild,
  OnInit,
} from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { SearchbarComponent, UiInput, UserCardComponent } from "@sports-ui/ui";
import { SendVotesFacade, SendVotesUserInfo } from "@sports-ui/send-votes-data-access";
import { OrganizationFeatureService } from "@sports-ui/organization-data-access";
import { ModalService } from "@sports-ui/modal-service";

@Component({
  selector: "lib-feature-send-votes",
  imports: [CommonModule, FormsModule, SearchbarComponent, UiInput, UserCardComponent],
  templateUrl: "./feature-send-votes.html",
  styleUrl: "./feature-send-votes.css",
})
export class FeatureSendVotes implements OnInit {
  protected facade = inject(SendVotesFacade);
  private organizationFacade = inject(OrganizationFeatureService);
  protected modal = inject(ModalService);

  searchTerm = signal("");

  @ViewChild("sendVotesModal") sendVotesModal!: TemplateRef<{
    user: SendVotesUserInfo;
    submit: (p: any) => void;
  }>;

  filteredUsers = computed(() => {
    const t = this.searchTerm().trim().toLowerCase();
    const users = this.facade.users();
    if (!t) return users;
    return users.filter(
      (u) =>
        u.fullName?.toLowerCase().includes(t) ||
        u.email.toLowerCase().includes(t)
    );
  });

  ngOnInit(): void {
    this.facade.loadUsers();
  }

  onSearch(term: string) {
    this.searchTerm.set(term ?? "");
  }

  open(user: SendVotesUserInfo) {
    this.modal.open({
      title: "Send Votes",
      width: "md",
      content: this.sendVotesModal,
      context: {
        user,
        submit: async (payload: { user: SendVotesUserInfo; amount: number }) => {
          try {
            const orgId = this.organizationFacade.selectedOrganization()?.id || "";
            await this.facade.sendVotes(payload.user.id, payload.amount, orgId);
            this.modal.close();
          } catch {
            // already toasted
          }
        },
      },
    });
  }
}
